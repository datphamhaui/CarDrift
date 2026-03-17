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

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Spin
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // Bob up and down
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = pos;
    }

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
