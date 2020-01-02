
using UnityEngine;
using UnityEngine.Events;

public class JumpComp : CharacterMovementComponent
{
    public KeyCode Key = KeyCode.Space;
    public float JumpVel = 5f;

    public UnityAction OnJump;

    private GravityMoveComp gravity;
    private WallRunComp wallRun;

    private void Awake()
    {
        gravity = GetComponent<GravityMoveComp>();
        wallRun = GetComponent<WallRunComp>();
    }

    public override Vector3 MoveUpdate(CustomCharacterController controller)
    {
        if(controller.IsGrounded && Input.GetKeyDown(Key))
        {
            gravity.GravityVel = controller.HeadYaw.up * JumpVel;
            OnJump?.Invoke();
        }

        if(!controller.IsGrounded && Input.GetKeyDown(Key) && controller.JumpsInAir < controller.MaxAirJumps)
        {
            gravity.GravityVel = controller.HeadYaw.up * JumpVel;
            controller.JumpsInAir++;
            OnJump?.Invoke();
        }

        if(wallRun != null && wallRun.IsWallRunning)
        {
            if (Input.GetKeyDown(Key))
            {
                gravity.GravityVel = controller.HeadYaw.up * JumpVel * 1.5f + (wallRun.IsRightWall ? wallRun.rightNormal : wallRun.leftNormal) * 9f + controller.HeadYaw.forward * 6f;
                OnJump?.Invoke();
            }
        }

        return Vector3.zero;
    }
}
