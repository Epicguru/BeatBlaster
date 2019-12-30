
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    public Vector3 TargetOffset;
    public bool DrawDebug = false;
    public float Depth = 0.2f;
    public ExtraDistanceResolveMode DstResolveMode = ExtraDistanceResolveMode.Rewind;
    public AnimationCurve SurfaceAngleDistanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private CustomCollisionResolver resolver;
    private Vector3 finalPos;

    private void CreateResolver()
    {
        resolver = new CustomCollisionResolver(GetComponent<Collider>());
        resolver.SurfaceAngleDistanceCurve = SurfaceAngleDistanceCurve;
    }

    private void Update()
    {
        if (resolver == null)
            CreateResolver();

        resolver.FastResolveDepth = Depth;
        resolver.DistanceCheckMode = DstResolveMode;

        resolver.DrawDebug = DrawDebug;
        var result = resolver.ResolveFast(transform.position, transform.position + TargetOffset, 5f);
        if (result.HasError)
            Debug.LogWarning(result.Error);
        else
            finalPos = result.FinalPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 0, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + TargetOffset);
        Gizmos.DrawCube(transform.position + TargetOffset, Vector3.one * 0.15f);
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(finalPos, Vector3.one * 0.1f);
    }
}
