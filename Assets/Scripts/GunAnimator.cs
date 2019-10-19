using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    [Header("References")]
    public GunSlideController Slide;
    public Animator Anim;

    [Header("Timing")]
    public Vector2 ADSTimes = new Vector2(0.35f, 0.5f);
    public Vector2 CrouchTimes = new Vector2(0.35f, 0.35f);

    [Header("Controls")]
    public bool Run = false;
    public bool ADS = false;
    public bool Crouch = false;
    public bool IsEmpty = false;
    public bool Reload = false;
    public bool CheckMagazine = false;
    public bool Shoot = false;
    public bool Chamber = false;

    [Header("States")]
    public bool IsReloading = false;
    public bool IsCheckingMagazine = false;
    public bool IsChambering = false;
    [Range(0f, 1f)]
    public float ADSLerp = 0f;
    [Range(0f, 1f)]
    public float CrouchLerp = 0f;

    private void Awake()
    {
        if(Slide == null)
        {
            Slide = GetComponentInChildren<GunSlideController>();
        }
    }

    private void LateUpdate()
    {
        Anim.SetBool("IsEmpty", IsEmpty);
        if (Chamber)
        {
            Chamber = false;
            if (!IsChambering)
            {
                IsChambering = true;
                Anim.SetTrigger("Chamber");
            }
        }

        if (Reload)
        {
            Reload = false;
            if(!IsReloading && !IsCheckingMagazine)
            {
                IsReloading = true;
                Anim.SetTrigger("Reload");
                if (IsEmpty)
                    IsChambering = true;
            }
        }

        if (CheckMagazine)
        {
            CheckMagazine = false;
            if(!IsReloading && !IsCheckingMagazine && !IsEmpty)
            {
                IsCheckingMagazine = true;
                Anim.SetTrigger("CheckMag");
            }
        }

        bool run = Run;
        bool ads = ADS;
        bool crouch = Crouch;
        if (IsReloading || IsCheckingMagazine)
        {
            run = false;
            ads = false;
            crouch = false;
        }
        else
        {
            if (Shoot)
            {
                Shoot = false;
                Anim.SetTrigger("Shoot");
            }
        }

        Anim.SetBool("Run", run);

        ADSLerp += (ads ? (1f / ADSTimes.x) : (-1f / ADSTimes.y)) * Time.deltaTime;
        CrouchLerp += (crouch ? (1f / CrouchTimes.x) : (-1f / CrouchTimes.y)) * Time.deltaTime;

        ADSLerp = Mathf.Clamp01(ADSLerp);
        CrouchLerp = Mathf.Clamp01(CrouchLerp);

        Anim.SetLayerWeight(1, CrouchLerp);
        Anim.SetLayerWeight(2, ADSLerp);
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string s = e.stringParameter.Trim().ToLower();

        switch (s)
        {
            case "reload":
                IsReloading = false;
                break;

            case "check mag":
            case "checkmag":
            case "check magazine":
            case "checkmagazine":
                IsCheckingMagazine = false;
                break;

            case "chamber":
                IsChambering = false;
                break;
        }
    }
}
