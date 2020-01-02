
using UnityEngine;

public class GravityMoveComp : CharacterMovementComponent
{
    public Vector3 GravityVel = Vector3.zero;

    public WallRunComp WallRun;

    public override Vector3 MoveUpdate(CustomCharacterController controller)
    {
        if(!(WallRun?.IsWallRunning ?? false))
            GravityVel += Time.deltaTime * controller.Gravity;

        if (controller.IsGrounded && !(WallRun?.IsWallRunning ?? false))
            GravityVel = controller.HeadYaw.up * -1f;

        return Vector3.zero;
    }

    public override Vector3 MoveLateUpdate(CustomCharacterController controller)
    {
        return GravityVel;
    }
}