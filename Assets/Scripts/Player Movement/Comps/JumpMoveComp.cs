
using UnityEngine;
using UnityEngine.Events;

public class JumpMoveComp : CharacterMovementComponent
{
    public KeyCode Key = KeyCode.Space;
    public float JumpVel = 5f;

    public UnityAction OnJump;

    private GravityMoveComp gravity;

    private void Awake()
    {
        gravity = GetComponent<GravityMoveComp>();
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

        return Vector3.zero;
    }
}
