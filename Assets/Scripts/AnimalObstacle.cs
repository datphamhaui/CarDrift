using UnityEngine;
using System.Collections;

/// <summary>
/// Gắn vào prefab con thú Suriyun.
/// Con thú TỰ ĐỘNG di chuyển liên tục ngay khi game bắt đầu — không cần chờ xe đến gần.
///
/// SETUP BẮT BUỘC:
///   1. Tag GameObject = "Obstacle"
///   2. Thêm Box Collider (isTrigger = TRUE) để phát hiện va chạm với xe
///   3. Kiểm tra tên integer parameter trong Animator window (thường là "anim")
///
/// ANIMATION INDEX (Suriyun TypeA):
///   0=Idle  1=Walk  2=Run  3=Jump  4=Eat  5=Rest  6=Attack  7=Damage  8=Die
/// </summary>
public class AnimalObstacle : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════════
    public enum MovementMode
    {
        JumpAcross,      // Nhảy arc ngang/chéo, lặp liên tục
        RunAcross,       // Chạy thẳng ngang, reset về gốc, lặp lại
        Zigzag,          // Chạy zíc-zắc, reset về gốc, lặp lại
        PopUpFromGround, // Trồi lên → đứng chặn → chui xuống → lặp
        DropFromSky,     // Rơi xuống → đứng chặn → bay lên → lặp
        Patrol,          // Đi qua đi lại liên tục
    }

    // ── Mode ──────────────────────────────────────────────────────────────────
    [Header("=== MOVEMENT MODE ===")]
    public MovementMode movementMode = MovementMode.JumpAcross;

    [Tooltip("Delay ngẫu nhiên trước khi bắt đầu (giây) — để các con thú không đồng bộ nhau")]
    public float randomStartDelay = 1.5f;

    // ── Animator ──────────────────────────────────────────────────────────────
    [Header("Animator (Suriyun TypeA)")]
    [Tooltip("Tên integer parameter trong Animator Controller (thường là 'anim')")]
    public string animParamName = "anim";
    public int animIdle = 0;
    public int animWalk = 1;
    public int animRun  = 2;
    public int animJump = 3;

    // ── Common ────────────────────────────────────────────────────────────────
    [Header("Common Settings")]
    [Tooltip("Nghỉ giữa mỗi chu kỳ di chuyển (giây)")]
    public float restBetweenCycles = 1.2f;

    [Tooltip("Damage gây ra khi xe đụng vào")]
    public int damage = 1;

    // ── JumpAcross ────────────────────────────────────────────────────────────
    [Header("JumpAcross Settings")]
    [Tooltip("Số cú nhảy mỗi chu kỳ")]
    public int   jumpCount           = 2;
    public float jumpDistance        = 5f;
    public float jumpHeight          = 1.5f;
    public float jumpDuration        = 0.5f;
    public float pauseBetweenJumps   = 0.3f;

    // ── RunAcross ─────────────────────────────────────────────────────────────
    [Header("RunAcross Settings")]
    public float runDistance = 12f;
    public float runSpeed    = 8f;

    // ── Zigzag ────────────────────────────────────────────────────────────────
    [Header("Zigzag Settings")]
    [Tooltip("Góc lệch chéo (độ)")]
    public float zigzagAngle    = 40f;
    public int   zigzagSegments = 3;
    public float zigzagSpeed    = 7f;

    // ── PopUpFromGround ───────────────────────────────────────────────────────
    [Header("PopUp / Drop Settings")]
    public float popUpDistance   = 2f;
    public float popUpDuration   = 0.5f;
    [Tooltip("Đứng chặn bao lâu trước khi chui xuống / bay lên lại (giây)")]
    public float surfaceWaitTime = 2f;

    // ── DropFromSky ───────────────────────────────────────────────────────────
    public float dropHeight   = 8f;
    public float dropDuration = 0.5f;

    // ── Patrol ────────────────────────────────────────────────────────────────
    [Header("Patrol Settings")]
    public float patrolDistance = 6f;
    public float patrolSpeed    = 3f;

    // ── Direction ─────────────────────────────────────────────────────────────
    [Header("Direction")]
    [Tooltip("Random hướng mỗi chu kỳ")]
    public bool  randomDirection    = true;
    [Tooltip("Ưu tiên ngang khi random (0=không bias, 1=luôn ngang)")]
    [Range(0f, 1f)]
    public float sidewaysBias       = 0.65f;
    [Tooltip("Hướng cố định (local space) khi randomDirection = false")]
    public Vector3 fixedLocalDirection = Vector3.right;

    // ── Internal ──────────────────────────────────────────────────────────────
    Animator _anim;
    Vector3  _spawnPos;

    static readonly Vector3[] LocalDirs =
    {
        new Vector3( 1, 0,  0),
        new Vector3(-1, 0,  0),
        new Vector3( 1, 0,  1).normalized,
        new Vector3(-1, 0,  1).normalized,
        new Vector3( 1, 0, -1).normalized,
        new Vector3(-1, 0, -1).normalized,
    };

    // ═══════════════════════════════════════════════════════════════════════════

    void Start()
    {
        _anim     = GetComponent<Animator>();
        _spawnPos = transform.position;

        InitByMode();
        StartCoroutine(MainLoop());
    }

    // ── Init vị trí ẩn ban đầu ────────────────────────────────────────────────
    void InitByMode()
    {
        switch (movementMode)
        {
            case MovementMode.PopUpFromGround:
                var p = _spawnPos; p.y -= popUpDistance;
                transform.position = p;
                SetAnim(animIdle);
                break;
            case MovementMode.DropFromSky:
                var s = _spawnPos; s.y += dropHeight;
                transform.position = s;
                SetAnim(animJump);
                break;
            default:
                SetAnim(animIdle);
                break;
        }
    }

    // ── Vòng lặp chính ────────────────────────────────────────────────────────
    IEnumerator MainLoop()
    {
        // Delay ngẫu nhiên ban đầu để các con thú không chạy đồng loạt
        yield return new WaitForSeconds(Random.Range(0f, randomStartDelay));

        while (true)
        {
            // Chờ game Playing
            yield return new WaitUntil(() => IsPlaying());

            switch (movementMode)
            {
                case MovementMode.JumpAcross:      yield return StartCoroutine(DoJumpAcross());      break;
                case MovementMode.RunAcross:       yield return StartCoroutine(DoRunAcross());       break;
                case MovementMode.Zigzag:          yield return StartCoroutine(DoZigzag());          break;
                case MovementMode.PopUpFromGround: yield return StartCoroutine(DoPopUp());           break;
                case MovementMode.DropFromSky:     yield return StartCoroutine(DoDropFromSky());     break;
                case MovementMode.Patrol:          yield return StartCoroutine(DoPatrol());          break;
            }

            // Nghỉ giữa chu kỳ (Patrol tự quản lý nên bỏ qua)
            if (movementMode != MovementMode.Patrol)
            {
                SetAnim(animIdle);
                yield return new WaitForSeconds(restBetweenCycles);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  MOVEMENT ROUTINES
    // ═══════════════════════════════════════════════════════════════════════════

    // ── 1. JumpAcross ─────────────────────────────────────────────────────────
    IEnumerator DoJumpAcross()
    {
        Vector3 dir = ChooseDirection();

        for (int i = 0; i < Mathf.Max(1, jumpCount); i++)
        {
            if (!IsPlaying()) yield break;

            SetAnim(animJump);
            Vector3 origin = transform.position;
            Vector3 target = origin + dir * jumpDistance;
            FaceDir(dir);

            yield return MoveArc(origin, target, jumpHeight, jumpDuration);
            transform.position = target;

            if (i < jumpCount - 1)
            {
                SetAnim(animIdle);
                yield return new WaitForSeconds(pauseBetweenJumps);
            }
        }
    }

    // ── 2. RunAcross: chạy sang → tức thì reset về gốc → lặp ─────────────────
    IEnumerator DoRunAcross()
    {
        SetAnim(animRun);
        Vector3 dir    = ChooseDirection();
        Vector3 target = _spawnPos + dir * runDistance;
        FaceDir(dir);

        float dist = 0f;
        while (dist < runDistance && IsPlaying())
        {
            float step = runSpeed * Time.deltaTime;
            transform.position += dir * step;
            dist += step;
            yield return null;
        }

        // Reset tức thì về spawn (con thú "biến mất" rồi xuất hiện lại)
        transform.position = _spawnPos;
    }

    // ── 3. Zigzag ─────────────────────────────────────────────────────────────
    IEnumerator DoZigzag()
    {
        SetAnim(animRun);
        Vector3 baseDir = ChooseDirection();

        for (int i = 0; i < zigzagSegments; i++)
        {
            if (!IsPlaying()) yield break;

            float angle  = (i % 2 == 0) ? zigzagAngle : -zigzagAngle;
            Vector3 seg  = Quaternion.AngleAxis(angle, Vector3.up) * baseDir;
            seg.y = 0; seg.Normalize();

            float segDist = runDistance / zigzagSegments;
            float traveled = 0f;
            FaceDir(seg);

            while (traveled < segDist && IsPlaying())
            {
                float step = zigzagSpeed * Time.deltaTime;
                transform.position += seg * step;
                traveled += step;
                yield return null;
            }
        }

        // Reset về spawn
        transform.position = _spawnPos;
    }

    // ── 4. PopUpFromGround: trồi lên → chờ → chui xuống ─────────────────────
    IEnumerator DoPopUp()
    {
        Vector3 hidden = _spawnPos; hidden.y -= popUpDistance;

        // Trồi lên
        SetAnim(animJump);
        yield return MoveTo(transform.position, _spawnPos, popUpDuration);
        transform.position = _spawnPos;

        // Đứng chặn
        SetAnim(animIdle);
        yield return new WaitForSeconds(surfaceWaitTime);

        // Chui xuống
        SetAnim(animJump);
        yield return MoveTo(_spawnPos, hidden, popUpDuration);
        transform.position = hidden;
    }

    // ── 5. DropFromSky: rơi xuống → chờ → bay lên ────────────────────────────
    IEnumerator DoDropFromSky()
    {
        Vector3 sky = _spawnPos; sky.y += dropHeight;

        // Rơi xuống (ease-in)
        SetAnim(animJump);
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            transform.position = Vector3.Lerp(sky, _spawnPos, t * t);
            yield return null;
        }
        transform.position = _spawnPos;

        // Đứng chặn
        SetAnim(animIdle);
        yield return new WaitForSeconds(surfaceWaitTime);

        // Bay lên (ease-out)
        SetAnim(animJump);
        elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            transform.position = Vector3.Lerp(_spawnPos, sky, Mathf.Sqrt(t));
            yield return null;
        }
        transform.position = sky;
    }

    // ── 6. Patrol: đi qua đi lại mãi mãi ────────────────────────────────────
    IEnumerator DoPatrol()
    {
        SetAnim(animWalk);
        Vector3 dir     = ChooseDirection();
        Vector3 pointA  = _spawnPos;
        Vector3 pointB  = _spawnPos + dir * patrolDistance;

        while (IsPlaying())
        {
            FaceDir(dir);
            yield return MoveLinear(transform.position, pointB, patrolSpeed);

            dir = -dir;
            FaceDir(dir);
            yield return MoveLinear(transform.position, pointA, patrolSpeed);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  UTILITY
    // ═══════════════════════════════════════════════════════════════════════════

    IEnumerator MoveArc(Vector3 from, Vector3 to, float height, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 p = Vector3.Lerp(from, to, t);
            p.y = from.y + height * Mathf.Sin(t * Mathf.PI);
            transform.position = p;
            yield return null;
        }
    }

    IEnumerator MoveTo(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
    }

    IEnumerator MoveLinear(Vector3 from, Vector3 to, float speed)
    {
        float duration = Vector3.Distance(from, to) / Mathf.Max(speed, 0.01f);
        float elapsed  = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        transform.position = to;
    }

    Vector3 ChooseDirection()
    {
        Vector3 local;
        if (!randomDirection)
            local = fixedLocalDirection.normalized;
        else if (Random.value < sidewaysBias)
            local = (Random.value < 0.5f) ? LocalDirs[0] : LocalDirs[1];
        else
            local = LocalDirs[Random.Range(0, LocalDirs.Length)];

        Vector3 world = transform.TransformDirection(local);
        world.y = 0;
        return world.normalized;
    }

    void FaceDir(Vector3 dir)
    {
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void SetAnim(int index)
    {
        if (_anim != null)
            _anim.SetInteger(animParamName, index);
    }

    bool IsPlaying() =>
        GameManager.Instance != null &&
        GameManager.Instance.CurrentState == GameManager.GameState.Playing;

    // ── Damage ────────────────────────────────────────────────────────────────
    // Dùng OnCollisionEnter (Box Collider non-trigger + Rigidbody isKinematic)
    // → xe bị chặn vật lý VÀ vẫn bị trừ máu
    void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        if (!IsPlaying()) return;
        GameManager.Instance.TakeDamage(damage);
    }

    // Giữ OnTriggerEnter phòng trường hợp vẫn muốn dùng trigger collider
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!IsPlaying()) return;
        GameManager.Instance.TakeDamage(damage);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (!randomDirection)
        {
            Vector3 dir = transform.TransformDirection(fixedLocalDirection.normalized);
            dir.y = 0;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, dir * 4f);
        }

        // Hiển thị patrol range
        if (movementMode == MovementMode.Patrol)
        {
            Vector3 dir = transform.TransformDirection(fixedLocalDirection.normalized);
            dir.y = 0;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + dir * patrolDistance);
        }
    }
}
