using UnityEngine;

/// <summary>
/// Attach to fuel can prefab. Requires a trigger Collider.
///
/// Setup:
///   - Add a Collider (isTrigger = true)
///   - Add a Rigidbody (isKinematic = true) on the car or fuel object
/// </summary>
public class FuelCollectible : MonoBehaviour
{
    [Tooltip("Amount of fuel restored on pickup")]
    public float fuelAmount = 20f;
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

        GameManager.Instance.AddFuel(fuelAmount);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayFuelPickup();

        Destroy(gameObject);
    }
}
