using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    [Header("References")]
    public GunSlideController Slide;
    public Animator Anim;

    [Header("Timing")]
    public Vector2 ADSTimes = new Vector2(0.15f, 0.2f);
    public Vector2 CrouchTimes = new Vector2(0.15f, 0.2f);

    [Header("Controls")]
    public bool IsRecursiveReload = false;
    public bool Run = false;
    public bool ADS = false;
    public bool Crouch = false;
    public bool IsEmpty = false;
    public bool Reload = false;
    public bool StopRecursiveReload = false;
    public bool CheckMagazine = false;
    public bool Shoot = false;
    public bool Chamber = false;
    public bool Inspect = false;

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
        if (!Anim.enabled || !Anim.gameObject.activeInHierarchy)
            return;

        Anim.SetBool("IsEmpty", IsEmpty);
        if (Chamber)
        {
            if (!IsChambering)
            {
                IsChambering = true;
                Anim.SetTrigger("Chamber");
            }
        }
        else
        {
            IsChambering = false;
            Anim.ResetTrigger("Chamber");
        }

        if (Inspect)
        {
            Inspect = false;
            Anim.SetTrigger("Inspect");
        }

        if (Reload)
        {
            Reload = false;
            if(!IsReloading && !IsCheckingMagazine)
            {
                IsReloading = true;

                if (!IsRecursiveReload)
                {
                    Anim.SetTrigger("Reload");
                }
                else
                {
                    Anim.SetBool("Reload", true);
                }
                if (IsEmpty)
                {
                    IsChambering = true;
                }
            }
        }
        if (StopRecursiveReload)
        {
            StopRecursiveReload = false;

            if (IsRecursiveReload && IsReloading)
            {
                Anim.SetBool("Reload", false);
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

        if (Shoot)
        {
            Shoot = false;

            if(IsRecursiveReload || (!IsReloading && !IsCheckingMagazine))
            {
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

    public void Equip()
    {
        IsReloading = false;
        IsChambering = false;
        IsCheckingMagazine = false;
        Anim.SetTrigger("Equip");
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string s = e.stringParameter.Trim().ToLower();

        switch (s)
        {
            case "reload":
                if(!IsRecursiveReload)
                    IsReloading = false;
                break;
            case "stop reload":
            case "stopreload":
            case "end reload":
            case "endreload":
            case "reload stop":
            case "reloadstop":
            case "reload end":
            case "reloadend":
                if (IsRecursiveReload)
                {
                    IsReloading = false;
                    Anim.SetBool("Reload", false);
                }
                else
                {
                    Debug.LogError("Should not be using stop/end reload when the gun is not a recursive reload. Use 'reload' instead.");
                }
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
