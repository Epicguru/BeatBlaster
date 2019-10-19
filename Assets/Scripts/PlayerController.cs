using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController Controller;
    public Transform HeadHeight;
    public ItemAngleOffset ItemOffset;
    public GunController Gun;

    [Header("Movement")]
    public float GravityForce = 9.81f;
    public Vector3 Velocity;
    public bool OnFloor = false;
    public float JumpVelocity = 5f;
    public float Speed = 3f, RunSpeed = 7f;
    public bool IsRunning = false;

    [Header("Crouching")]
    public Vector2 CameraHeights = new Vector2(0.5f, 1f);
    [Range(0f, 1f)]
    public float StandingLerp = 1f;
    public Vector2 StandingTime = new Vector2(0.3f, 0.4f);

    private float verticalVel;

    private void Update()
    {
        if(OnFloor && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVel = JumpVelocity;
        }

        IsRunning = Input.GetKey(KeyCode.LeftShift);
        ItemOffset.Anim.SetBool("Run", IsRunning);

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

        StandingLerp += (!Input.GetKey(KeyCode.LeftControl) ? (1f / StandingTime.y) : (-1f / StandingTime.x)) * Time.deltaTime;
        StandingLerp = Mathf.Clamp01(StandingLerp);
        HeadHeight.transform.localPosition = new Vector3(0f, Mathf.Lerp(CameraHeights.x, CameraHeights.y, StandingLerp));

        Vector3 worldSpace = transform.TransformDirection(flatDir);
        verticalVel += GravityForce * Time.deltaTime;

        Velocity = worldSpace * (IsRunning ? RunSpeed : Speed);
        Velocity += new Vector3(0f, verticalVel, 0f);

        var flags = Controller.Move(Velocity * Time.deltaTime);

        OnFloor = false;
        if (flags.HasFlag(CollisionFlags.Below))
        {
            Velocity.y = -0.1f;
            OnFloor = true;
        }
    }
}
