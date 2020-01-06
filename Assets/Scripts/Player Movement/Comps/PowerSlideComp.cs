
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
    public bool IsInSlide = false;

    public UnityAction OnSlideStart;
    private float timer = 0f;

    public override void MoveUpdate(CustomCharacterController controller)
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
            timer = 0f;

        bool isRunning = controller.IsRunning;

        if(controller.IsGrounded && isRunning && !controller.IsCrouching && Input.GetKeyDown(KeyCode.C))
        {
            // Crouch has just started while running. Powerslide time!
            timer = MinSlideDuration;
            AdditionalVel = BaseSpeedAddition;
            IsInSlide = true;
            OnSlideStart?.Invoke();
        }

        if(timer > 0f)
        {
            controller.TargetCrouchLerp = 1f;
            IsInSlide = false;
        }

        AdditionalVel -= SpeedDecayRate * Time.deltaTime;
        if (AdditionalVel < 0f)
            AdditionalVel = 0f;
    }

    public override Vector3 MoveFixedUpdate(CustomCharacterController c)
    {
        return c.HeadYaw.forward * AdditionalVel;
    }
}