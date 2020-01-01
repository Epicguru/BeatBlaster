
using UnityEngine;

public class WallRunComp : CharacterMovementComponent
{
    public CustomCharacterController Controller
    {
        get
        {
            if (_controller == null)
                _controller = GetComponent<CustomCharacterController>();
            return _controller;
        }
    }
    private CustomCharacterController _controller;

    public bool IsWallRunning = false;
    public bool IsRightWall = false;

    public bool IsWallToRight = false;
    public bool IsWallToLeft = false;

    public float WallDetectDistance = 0.9f;
    public float WallDetectHeightOffset = 0f;

    [Header("Runtime")]
    public Vector3 worldNormal;

    public LayerMask Mask;

    public override Vector3 MoveUpdate(CustomCharacterController controller)
    {
        Vector3 start = transform.position + Controller.HeadYaw.up * WallDetectHeightOffset;

        IsWallToRight = Physics.Raycast(new Ray(start, Controller.HeadYaw.right), out RaycastHit rightRay, WallDetectDistance, Mask, QueryTriggerInteraction.Ignore);
        IsWallToLeft = Physics.Raycast(new Ray(start, -Controller.HeadYaw.right), out RaycastHit leftRay, WallDetectDistance, Mask, QueryTriggerInteraction.Ignore);

        if (IsWallToLeft)
        {
            worldNormal = leftRay.normal;
            IsRightWall = false;
        }
        if (IsWallToRight)
        {
            worldNormal = rightRay.normal;
            IsRightWall = true;
        }

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 start = transform.position + Controller.HeadYaw.up * WallDetectHeightOffset;

        Gizmos.color = IsWallToRight ? Color.green : Color.red;
        Gizmos.DrawLine(start, start + Controller.HeadYaw.right * WallDetectDistance);

        Gizmos.color = IsWallToLeft ? Color.green : Color.red;
        Gizmos.DrawLine(start, start + -Controller.HeadYaw.right * WallDetectDistance);
    }
}
