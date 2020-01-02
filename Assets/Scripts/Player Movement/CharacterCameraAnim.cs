
using UnityEngine;

public class CharacterCameraAnim : MonoBehaviour
{
    public CustomCharacterController Controller
    {
        get
        {
            if (_controller == null)
                _controller = GetComponent<CustomCharacterController>();
            return _controller;
        }
    }
    private CustomCharacterController _controller;

    public Animator Anim;
    public PowerSlideComp SlideComp;
    public JumpComp JumpComp;

    private void Awake()
    {
        if(SlideComp != null)
            SlideComp.OnSlideStart += PowerSlideStart;
        if (JumpComp != null)
            JumpComp.OnJump += JumpStart;

        Controller.OnEnterGround += OnEnterGround;
    }

    private void PowerSlideStart()
    {
        Anim.SetTrigger("Slide");
    }

    private void JumpStart()
    {
        Anim.SetTrigger("JumpStart");
    }

    private void JumpEnd()
    {
        Anim.SetTrigger("JumpEnd");
    }

    private void OnEnterGround()
    {
        Vector3 vel = Controller.CurrentRealVelocity;

        const float minFallVel = 5f;

        const float MIN_VEL = minFallVel * minFallVel;
        if(vel.sqrMagnitude >= MIN_VEL)
        {
            JumpEnd();
        }
    }

    private void Update()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && Controller.IsGrounded && Controller.RealCrouchLerp < 0.4f;
        bool isWalking = Controller.CurrentRealVelocity.sqrMagnitude > 5f && Controller.IsGrounded;

        Anim.SetBool("Walk", isWalking);
        Anim.SetBool("Run", isRunning);

        //Debug.Log(Controller.CurrentRealVelocity + ": " + Controller.CurrentRealVelocity.magnitude);
    }
}
