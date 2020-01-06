
using UnityEngine;
using UnityEngine.Events;

public class JumpComp : CharacterMovementComponent
{
    public KeyCode Key = KeyCode.Space;
    public float JumpVel = 5f;

    public UnityAction OnJump;

    private bool jumpPending = false;

    public override void MoveUpdate(CustomCharacterController c)
    {
        bool canJump = c.IsGrounded || c.JumpsInAir < c.MaxAirJumps;

        if(canJump && Input.GetKeyDown(Key))
        {
            jumpPending = true;
            OnJump?.Invoke();
            if(!c.IsGrounded)
                c.JumpsInAir++;
        }
    }

    public override Vector3 MoveFixedUpdate(CustomCharacterController c)
    {
        if (!jumpPending)
            return Vector3.zero;

        if (c.IsGrounded)
        {
            //jumpPending = false;
            return -c.Gravity.normalized * JumpVel;
        }
        else
        {
            jumpPending = false;
            //OnJump?.Invoke();
            c.SetDirectionalVelocity(Vector3.up * JumpVel);
            return Vector3.zero;
        }
    }
}
