using UnityEngine;

/// <summary>
/// Attach to obstacle prefab. Requires a trigger Collider.
///
/// Setup:
///   - Add a Collider (isTrigger = true)
///   - Tag obstacle as "Obstacle"
///   - Car must have tag "Player"
/// </summary>
public class ObstacleCollision : MonoBehaviour
{
    [Tooltip("Damage dealt to the player on contact")]
    public int damage = 1;
    public float rotateSpeed = 90f;
    public float bobAmplitude = 0.3f;
    public float bobSpeed = 2f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        GameManager.Instance.TakeDamage(damage);
        Destroy(gameObject);
    }
}
