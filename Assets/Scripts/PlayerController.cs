using UnityEngine;

[ExecuteInEditMode]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public ItemAngleOffset ItemOffset;
    public GunController Gun;
    public Camera MainCamera;
    public CameraLook CamLook;
    public AudioSource AudioSource;
    public Transform CameraOffset;
    public CustomCharacterController CC;

    [Header("Camera")]
    public float DefaultFOV = 65f;

    public bool IsRunning
    {
        get
        {
            return Input.GetKey(KeyCode.LeftShift) && CC.IsGrounded && CC.CurrentRealVelocity.sqrMagnitude > 25f;
        }
    }

    public bool IsCrouching
    {
        get
        {
            return CC.RealCrouchLerp == 1f || Input.GetKey(KeyCode.C);
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        float fov = MainCamera.fieldOfView;
        float targetFov = Mathf.Lerp(DefaultFOV, DefaultFOV / Gun.ADSZoom, Gun.ADSLerp);
        if (fov != targetFov)
            MainCamera.fieldOfView = targetFov;

        CamLook.InternalSens = Mathf.Lerp(1f, (1f / Gun.ADSZoom), Gun.ADSLerp);

        Vector3 flatDir = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
            flatDir.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            flatDir.x += 1f;
        if (Input.GetKey(KeyCode.S))
            flatDir.z -= 1f;
        if (Input.GetKey(KeyCode.W))
            flatDir.z += 1f;
        flatDir.Normalize();
        ItemOffset.Anim.SetBool("Walk", flatDir != Vector3.zero);

        ItemOffset.AnimScale = 0.1f + (1f - Gun.ADSLerp) * 0.9f;
        ItemOffset.Scale = 0.1f + (1f - Gun.ADSLerp) * 0.9f;       
    }

    private void LateUpdate()
    {
        if((Gun?.CameraAnim ?? null) != null)
        {
            CameraOffset.localPosition = Gun.CameraAnim.localPosition;
            CameraOffset.localRotation = Gun.CameraAnim.localRotation;
        }
        else
        {
            CameraOffset.localPosition = Vector3.zero;
            CameraOffset.localRotation = Quaternion.identity;
        }        
    }
}
