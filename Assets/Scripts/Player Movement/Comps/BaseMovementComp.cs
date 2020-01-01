
using UnityEngine;

public class BaseMovementComp : CharacterMovementComponent
{
    public float Speed = 4.5f, RunSpeed = 8f;
    public Vector3 CurrentVel = Vector3.zero;

    public override Vector3 MoveUpdate(CustomCharacterController controller)
    {
        if (!controller.IsFlying)
        {
            CurrentVel = Vector3.zero; Vector3 inputRaw = Vector3.zero;

            if (Input.GetKey(KeyCode.A))
                inputRaw.x -= 1f;
            if (Input.GetKey(KeyCode.D))
                inputRaw.x += 1f;
            if (Input.GetKey(KeyCode.S))
                inputRaw.z -= 1f;
            if (Input.GetKey(KeyCode.W))
                inputRaw.z += 1f;
            inputRaw.Normalize();

            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float speed = isRunning ? RunSpeed : Speed;

            Vector3 final = Vector3.zero;
            final += controller.HeadYaw.forward * inputRaw.z * speed;
            final += controller.HeadYaw.right * inputRaw.x * speed;

            CurrentVel += final;
        }

        return CurrentVel;
    }
}
