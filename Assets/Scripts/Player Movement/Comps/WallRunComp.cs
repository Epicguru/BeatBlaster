
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

    [Header("Speeds")]
    public float MinStartSpeed = 7f;
    public Vector3 Speeds;

    [Header("Runtime")]
    public Vector3 rightNormal;
    public Vector3 leftNormal;

    public LayerMask Mask;

    public override Vector3 MoveUpdate(CustomCharacterController controller)
    {
        Vector3 start = transform.position + Controller.HeadYaw.up * WallDetectHeightOffset;

        IsWallToRight = Physics.Raycast(new Ray(start, Controller.HeadYaw.right), out RaycastHit rightRay, WallDetectDistance, Mask, QueryTriggerInteraction.Ignore);
        IsWallToLeft = Physics.Raycast(new Ray(start, -Controller.HeadYaw.right), out RaycastHit leftRay, WallDetectDistance, Mask, QueryTriggerInteraction.Ignore);

        leftNormal = leftRay.normal;
        rightNormal = rightRay.normal;

        if (controller.IsGrounded || controller.CurrentRealVelocity.sqrMagnitude < MinStartSpeed * MinStartSpeed || !Input.GetKey(KeyCode.W) || (IsWallRunning && IsRightWall && !IsWallToRight) || (IsWallRunning && !IsRightWall && !IsWallToLeft))
        {
            IsWallRunning = false;
            return Vector3.zero;
        }

        if (!IsWallRunning && !controller.IsGrounded && controller.CurrentRealVelocity.sqrMagnitude >= MinStartSpeed * MinStartSpeed && Input.GetKey(KeyCode.W))
        {
            if (IsWallToLeft)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    IsWallRunning = true;
                    IsRightWall = false;
                    GetComponent<GravityMoveComp>().GravityVel = Vector3.zero;
                }
            }
            if (IsWallToRight)
            {
                if (Input.GetKey(KeyCode.D))
                {
                    IsWallRunning = true;
                    IsRightWall = true;
                    GetComponent<GravityMoveComp>().GravityVel = Vector3.zero;
                }
            }            
        }

        if (IsWallRunning)
        {
            if (IsRightWall)
            {
                return controller.HeadYaw.forward * Speeds.z - rightNormal * Speeds.x + controller.HeadYaw.up * Speeds.y;
            }
            else
            {
                return controller.HeadYaw.forward * Speeds.z - leftNormal * Speeds.x + controller.HeadYaw.up * Speeds.y;
            }
        }
        else
        {
            return Vector3.zero;
        }
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
