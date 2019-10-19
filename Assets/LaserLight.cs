using UnityEngine;

[ExecuteInEditMode]
public class LaserLight : MonoBehaviour
{
    public LineRenderer Line;
    public LayerMask CollisionMask;
    public float MaxDistance = 100f;

    private void Awake()
    {
        Line.positionCount = 2;
        Line.useWorldSpace = true;
        Line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
    }

    private void LateUpdate()
    {
        Line.SetPosition(0, transform.position);
        var worked = Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, MaxDistance, CollisionMask);
        if (!worked)
        {
            Line.SetPosition(1, transform.position + transform.forward * MaxDistance);
        }
        else
        {
            Line.SetPosition(1, hit.point);
        }
    }
}
