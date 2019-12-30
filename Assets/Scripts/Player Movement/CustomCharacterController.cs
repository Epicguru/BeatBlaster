
using UnityEngine;

/// <summary>
/// Intended as a drop-in replacement for the standard CharacterController, which lacks key features such as variable gravity or player tilt.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class CustomCharacterController : MonoBehaviour
{
    private static Collider[] Overlaps = new Collider[64];

    public CapsuleCollider Collider
    {
        get
        {
            if (_collider == null)
                _collider = GetComponent<CapsuleCollider>();
            return _collider;
        }
    }
    private CapsuleCollider _collider;

    public CustomCollisionResolver Resolver { get; private set; }

    [Header("References")]
    public Transform Head;
    public Transform HeadYaw;

    [Header("Collider")]
    public bool ColliderIsTrigger = true;

    [Header("Movement Settings")]
    public float BaseSpeed = 8f;
    public float RunSpeed = 14f;
    public float JumpVel = 4f;

    [Header("Crouching")]
    public float RegularHeight = 1.9f;
    public float CrouchHeight = 0.9f;
    [Range(0f, 1f)]
    public float TargetCrouchLerp = 0f;

    [Header("Grounding")]
    [Range(0f, 1f)]
    public float FeetDetectorScale = 0.9f;
    [Range(0f, 0.5f)]
    public float FeetDetectorOffset = 0.05f;

    [Header("Gravity")]
    public Vector3 Gravity = new Vector3(0f, -9.81f, 0f);

    [Header("NoClip")]
    public bool NoClip = false;
    public float NoClipSpeed = 8f;
    public float NoClipBoostSpeed = 15f;

    [Header("Runtime")]
    public Vector3 ForcesVelocity = Vector3.zero;
    public Vector3 GravityVelocity = Vector3.zero;
    public bool IsGrounded = false;

    [Header("Other")]
    public ExtraDistanceResolveMode DistanceResolveMode = ExtraDistanceResolveMode.Rewind;
    public AnimationCurve SurfaceAngleDistanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float FastResolveDepth = 0.02f;

    [Header("Debug")]
    public bool DrawFeetDetector = true;
    public bool DrawMoveDebug = false;

    private void Awake()
    {
        Resolver = new CustomCollisionResolver(Collider);
        Resolver.FastResolveDepth = Resolver.RecommendedFastResolveDepth;
        Resolver.SurfaceAngleDistanceCurve = SurfaceAngleDistanceCurve;
    }

    private void Update()
    {
        Resolver.DrawDebug = DrawMoveDebug;
        Resolver.DistanceCheckMode = DistanceResolveMode;
        Resolver.FastResolveDepth = FastResolveDepth;

        if (Input.GetKeyDown(KeyCode.N))
            NoClip = !NoClip;

        if (NoClip)
        {
            MoveNoClip();
        }
        else
        {
            UpdateCrouching();
            MoveRegular();
        }
    }

    private void UpdateCrouching()
    {
        Collider.height = Mathf.Lerp(RegularHeight, CrouchHeight, TargetCrouchLerp);
        Collider.center = Vector3.Lerp(Vector3.zero, new Vector3(0f, TargetCrouchLerp, 0f), TargetCrouchLerp);

    }

    private void MoveRegular()
    {
        // Face downwards towards gravity.
        //transform.up = Gravity.sqrMagnitude < 0.001f ? Vector3.up : -Gravity;
        const float MAX_SPEED = 400f;
        const float MIN_SPEED = 90f;
        const float MAX_SPEED_ANGLE = 180f;
        Quaternion target = Quaternion.LookRotation(Vector3.forward, -Gravity);
        float p = Mathf.Clamp01(Quaternion.Angle(transform.localRotation, target) / MAX_SPEED_ANGLE);
        float angleSpeed = Mathf.Lerp(MIN_SPEED, MAX_SPEED, p);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, target, angleSpeed * Time.deltaTime);

        // Update forces vel with current gravity;
        if (!IsGrounded)
            GravityVelocity += Gravity * Time.deltaTime;
        else
            GravityVelocity = Vector3.zero;

        if(Collider.isTrigger != ColliderIsTrigger)
            Collider.isTrigger = ColliderIsTrigger;

        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.A))        
            input.x -= 1f;
        
        if (Input.GetKey(KeyCode.D))        
            input.x += 1f;
        
        if (Input.GetKey(KeyCode.S))        
            input.y -= 1f;
        
        if (Input.GetKey(KeyCode.W))        
            input.y += 1f;

        if (IsGrounded && Input.GetKeyDown(KeyCode.Space))
            AddJumpVelocity(JumpVel);

        Vector3 worldDir = Vector3.zero;
        worldDir += HeadYaw.right * input.x;
        worldDir += HeadYaw.forward * input.y;
        worldDir.Normalize();

        float speed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : BaseSpeed;

        Vector3 offset = worldDir * speed * Time.deltaTime;
        offset += GravityVelocity * Time.deltaTime;
        offset += ForcesVelocity * Time.deltaTime;

        if (DrawMoveDebug)
        {
            Debug.DrawLine(transform.position, transform.position + offset, Color.black);
            Debug.DrawLine(transform.position + offset, transform.position + offset + Vector3.up * 0.01f, Color.black);

        }

        var result = Resolver.ResolveFast(transform.position, transform.position + offset);
        if (!result.HasError)
            transform.position = result.FinalPosition;
        else
            Debug.LogWarning(result.Error);

        // Detect the ground...
        UpdateGroundInfo();
    }

    public void AddJumpVelocity(float length)
    {
        GravityVelocity = -Gravity.normalized * length;
    }

    private void MoveNoClip()
    {
        Collider.isTrigger = true;

        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            direction -= Head.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Head.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction -= Head.forward;
        }
        if (Input.GetKey(KeyCode.W))
        {
            direction += Head.forward;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction -= Head.up;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += Head.up;
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? NoClipBoostSpeed : NoClipSpeed;
        Vector3 offset = direction.normalized * speed * Time.deltaTime;

        transform.position += offset;
    }    

    private void UpdateGroundInfo()
    {
        IsGrounded = false;

        int hits = Physics.OverlapSphereNonAlloc(GetFeetDetectorPosition(), GetFeetDetectorRadius(), Overlaps);
        for (int i = 0; i < hits; i++)
        {
            var collider = Overlaps[i];
            if (DoesColliderCountForGrounded(collider))
            {
                IsGrounded = true;
                break;
            }
        }
    }

    public virtual bool DoesColliderCountForGrounded(Collider c)
    {
        if (c == null || c == this.Collider || c.isTrigger)
            return false;

        return true;
    }

    public Vector3 GetFeetDetectorPosition()
    {
        return transform.TransformPoint(Collider.center - Vector3.up * (Collider.height * 0.5f - Collider.radius + FeetDetectorOffset));
    }

    public float GetFeetDetectorRadius()
    {
        return Collider.radius * FeetDetectorScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (DrawFeetDetector)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(GetFeetDetectorPosition(), GetFeetDetectorRadius());
        }        
    }
}
