
using UnityEngine;
using UnityEngine.Events;

public class PowerSlideComp : CharacterMovementComponent
{
    [Header("Slide settings")]
    public float BaseSpeedAddition = 4f;
    public float SpeedDecayRate = 8f;
    public float MinSlideDuration = 0.4f;

    [Header("Runtime")]
    public float AdditionalVel = 0f;

    public UnityAction OnSlideStart;
    private float timer = 0f;

    public override Vector3 MoveUpdate(CustomCharacterController controller)
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
            timer = 0f;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if(controller.IsGrounded && isRunning && controller.RealCrouchLerp < 0.4f && Input.GetKeyDown(KeyCode.C))
        {
            // Crouch has just started while running. Powerslide time!
            timer = MinSlideDuration;
            AdditionalVel = BaseSpeedAddition;

            OnSlideStart?.Invoke();

        }

        if(timer > 0f)
        {
            controller.TargetCrouchLerp = 1f;
        }

        AdditionalVel -= SpeedDecayRate * Time.deltaTime;
        if (AdditionalVel < 0f)
            AdditionalVel = 0f;

        return controller.HeadYaw.forward * AdditionalVel;
    }
}