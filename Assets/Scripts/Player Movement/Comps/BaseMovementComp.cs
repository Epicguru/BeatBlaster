
using UnityEngine;

public class BaseMovementComp : CharacterMovementComponent
{
    [Header("Control")]
    public float Speed = 4.5f, RunSpeed = 8f;
    public float SpeedMultiplier = 1f;

    [Header("Runtime")]
    public Vector3 RawInput;
    public Vector3 FinalVel;

    public override void MoveUpdate(CustomCharacterController c)
    {
        RawInput = Vector3.zero;
        FinalVel = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
            RawInput.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            RawInput.x += 1f;
        if (Input.GetKey(KeyCode.S))
            RawInput.z -= 1f;
        if (Input.GetKey(KeyCode.W))
            RawInput.z += 1f;
        RawInput.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && (!c.IsCrouching || (GetMoveComp<PowerSlideComp>()?.IsInSlide ?? false));
        c.IsRunning = isRunning;
        float speed = isRunning ? RunSpeed : Speed;

        Vector3 final = Vector3.zero;
        final += c.HeadYaw.forward * RawInput.z * speed;
        final += c.HeadYaw.right * RawInput.x * speed;

        FinalVel += final;
    }

    public override Vector3 MoveFixedUpdate(CustomCharacterController c)
    {
        if (c.IsGrounded)
        {
            return FinalVel * SpeedMultiplier;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
