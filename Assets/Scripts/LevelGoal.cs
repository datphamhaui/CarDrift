using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[LevelGoal] OnTriggerEnter: {other.name}, tag={other.tag}, GameState={GameManager.Instance?.CurrentState}");

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("[LevelGoal] Player reached goal! Calling TriggerWin.");
            GameManager.Instance.TriggerWin();
        }
    }
}
