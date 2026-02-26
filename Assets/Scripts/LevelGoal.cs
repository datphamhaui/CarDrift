using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TriggerWin();
        }
    }
}
