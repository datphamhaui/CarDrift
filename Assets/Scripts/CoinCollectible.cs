using UnityEngine;

/// <summary>
/// Attach to coin prefab. Requires a trigger Collider.
/// Rotates the coin and awards points on pickup.
///
/// Setup:
///   - Tag the coin prefab as "Coin"
///   - Add a SphereCollider (isTrigger = true)
///   - Add a Rigidbody (isKinematic = true) on the car or coin
/// </summary>
public class CoinCollectible : MonoBehaviour
{
    public float rotateSpeed = 180f;
    public float bobAmplitude = 0.3f;
    public float bobSpeed = 2f;

    void Start()  => CollectibleAnimator.Register(transform, rotateSpeed, bobAmplitude, bobSpeed);
    void OnDestroy() => CollectibleAnimator.Unregister(transform);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddCoin();

        if (AudioManager.instance != null)
            AudioManager.instance.PlayCoinPickup();

        Destroy(gameObject);
    }
}
