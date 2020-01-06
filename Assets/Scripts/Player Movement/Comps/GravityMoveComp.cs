
using UnityEngine;

public class GravityMoveComp : CharacterMovementComponent
{
    public float GroundedGravityVelocity = 0f;

    public override Vector3 MoveFixedUpdate(CustomCharacterController c)
    {
        if (c.IsGrounded)
        {
            if (GroundedGravityVelocity != 0f)
                return c.Gravity.normalized * GroundedGravityVelocity;
            else
                return Vector3.zero;
        }
        else
        {
            c.Body.velocity += c.Gravity * Time.fixedDeltaTime;
            return Vector3.zero;
        }
    }
}