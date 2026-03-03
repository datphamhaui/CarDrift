using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform[] nodes;
    [SerializeField] private int smoothness = 20;

    void Start()
    {
        DrawSmoothPath();
    }

    void DrawSmoothPath()
    {
        if (nodes.Length < 2) return;

        int totalPoints = (nodes.Length - 1) * smoothness + 1;
        lineRenderer.positionCount = totalPoints;

        int index = 0;
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            Vector3 p0 = nodes[i].position;
            Vector3 p1 = nodes[i + 1].position;
            Vector3 control = (p0 + p1) / 2 + Vector3.right * (i % 2 == 0 ? 1f : -1f);

            for (int j = 0; j < smoothness; j++)
            {
                float t = j / (float)smoothness;
                Vector3 point = Mathf.Pow(1 - t, 2) * p0
                              + 2 * (1 - t) * t * control
                              + Mathf.Pow(t, 2) * p1;

                lineRenderer.SetPosition(index, point);
                index++;
            }
        }

        lineRenderer.SetPosition(index, nodes[nodes.Length - 1].position);
    }
}