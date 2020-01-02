using UnityEngine;

public class PlayerLeaning : MonoBehaviour
{
    public PlayerController PlayerController
    {
        get
        {
            if (_pc == null)
                _pc = GetComponent<PlayerController>();
            return _pc;
        }
    }
    private PlayerController _pc;
    public Transform OffsetTransform;
    public Transform IndependentOffsetTransform;
    public Transform AngleTransform;

    [Header("Control")]
    [Range(-1f, 1f)]
    [Tooltip("Lean amount, where -1 is left and 1 is right, with 0 being neutral.")]
    public float LeanAmount = 0f;
    public AnimationCurve LeanCurve;
    public float LeanSpeed = 6f;

    [Header("Timing")]
    public float LeanOffset = 0.1f;
    public Vector2 LeanIdependentOffset = new Vector2(-0.2f, 0.2f);
    public Vector2 LeanIdependentOffsetCrouch = new Vector2(-0.1f, 0.1f);
    public float LeanAngle = 20f;

    public bool Left, Right;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (!Left)
            {
                Left = true;
                Right = false;
            }
            else
            {
                Left = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!Right)
            {
                Right = true;
                Left = false;
            }
            else
            {
                Right = false;
            }
        }
        if(PlayerController.IsRunning)
        {
            Left = false;
            Right = false;
        }

        if (Left)
        {
            LeanAmount -= LeanSpeed * Time.deltaTime;

        }
        else if (Right)
        {

            LeanAmount += LeanSpeed * Time.deltaTime;
        }
        else if(LeanAmount != 0f)
        {
            if(LeanAmount > 0f)
            {
                LeanAmount -= LeanSpeed * Time.deltaTime;
                if (LeanAmount < 0f)
                    LeanAmount = 0f;
            }
            else
            {
                LeanAmount += LeanSpeed * Time.deltaTime;
                if (LeanAmount > 0f)
                    LeanAmount = 0f;
            }
        }

        LeanAmount = Mathf.Clamp(LeanAmount, -1f, 1f);

        float lerp = LeanCurve.Evaluate(LeanAmount) * 0.5f + 0.5f;

        float angle = Mathf.Lerp(LeanAngle, -LeanAngle, lerp);
        float offset = Mathf.Lerp(-LeanOffset, LeanOffset, lerp);

        float indLerp = 2f * (lerp - 0.5f);
        float indOffset = 0f;
        float adsLerp = PlayerController.Gun?.ADSLerp ?? 0f;
        float crouchLerp = PlayerController.CC.RealCrouchLerp;
        if(indLerp > 0f)
        {
            indOffset = Mathf.Lerp(Mathf.Lerp(0f, LeanIdependentOffset.y, indLerp * (1f - adsLerp)), Mathf.Lerp(0f, LeanIdependentOffsetCrouch.y, indLerp * (1f - adsLerp)), crouchLerp);
        }
        else if(indLerp < 0f)
        {
            indOffset = Mathf.Lerp(Mathf.Lerp(0f, LeanIdependentOffset.x, -indLerp * (1f - adsLerp)), Mathf.Lerp(0f, LeanIdependentOffsetCrouch.x, -indLerp * (1f - adsLerp)), crouchLerp);
        }

        AngleTransform.localEulerAngles = new Vector3(0f, 0f, angle);
        IndependentOffsetTransform.localPosition = new Vector3(indOffset, 0f, 0f);
        OffsetTransform.localPosition = new Vector3(offset, 0f, 0f);
    }
}
