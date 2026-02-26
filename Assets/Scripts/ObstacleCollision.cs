using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    [Header("Settings")]
    public float minSpeedToTrigger = 5f;

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Wall"))
        {
            // Only trigger game over if moving fast enough
            var car = GetComponent<PrometeoCarController>();
            if (car != null && Mathf.Abs(car.carSpeed) < minSpeedToTrigger)
                return;

            GameManager.Instance.TriggerGameOver();
        }
    }
}
