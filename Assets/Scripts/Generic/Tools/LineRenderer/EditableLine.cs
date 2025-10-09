using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EditableLine : MonoBehaviour
{
    public Vector3[] points;
    private LineRenderer lr;

    void OnValidate()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null) return;

        if (points == null || points.Length != lr.positionCount)
        {
            points = new Vector3[lr.positionCount];
            lr.GetPositions(points);
        }
    }

    public void Apply()
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }

    public bool UsingWorldSpace()
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        return lr.useWorldSpace;
    }
}
