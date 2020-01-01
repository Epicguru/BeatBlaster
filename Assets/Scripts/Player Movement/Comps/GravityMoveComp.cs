
using UnityEngine;

public class GravityMoveComp : CharacterMovementComponent
{
    public Vector3 GravityVel = Vector3.zero;

    public override Vector3 MoveUpdate(CustomCharacterController controller)
    {
        GravityVel += Time.deltaTime * controller.Gravity;
        if (controller.IsGrounded)
            GravityVel = controller.HeadYaw.up * -1f;

        return Vector3.zero;
    }

    public override Vector3 MoveLateUpdate(CustomCharacterController controller)
    {
        return GravityVel;
    }
}