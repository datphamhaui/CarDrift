using UnityEngine;

/// <summary>
/// Attach to heart prefab. Requires a trigger Collider.
///
/// Setup:
///   - Add a Collider (isTrigger = true)
///   - Add a Rigidbody (isKinematic = true)
///   - Car must have tag "Player"
/// </summary>
public class HeartCollectible : MonoBehaviour
{
    [Tooltip("Amount of HP restored on pickup")]
    public int hpAmount = 1;
    public float rotateSpeed = 90f;
    public float bobAmplitude = 0.3f;
    public float bobSpeed = 2f;

    void Start()     => CollectibleAnimator.Register(transform, rotateSpeed, bobAmplitude, bobSpeed);
    void OnDestroy() => CollectibleAnimator.Unregister(transform);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        GameManager.Instance.AddHP(hpAmount);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayHeartPickup();

        Destroy(gameObject);
    }
}
