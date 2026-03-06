using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    [Header("Settings")]
    public float minSpeedToTrigger = 5f;
    public int damage = 1;

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Wall"))
        {
            var car = GetComponent<PrometeoCarController>();
            if (car != null && Mathf.Abs(car.carSpeed) < minSpeedToTrigger)
                return;

            GameManager.Instance.TakeDamage(damage);
        }
    }
}
