
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
    [Range(0f, 1f)]
    public float AdditionalForcesDecay = 0.95f;
    [Range(1, 200)]
    public int AdditionalForcesDecayTickRate = 120;

    [Header("Crouching")]
    public float CrouchSpeed = 4f;
    public float RegularHeight = 1.9f;
    public float CrouchHeight = 0.9f;
    [Range(0f, 1f)]
    public float TargetCrouchLerp = 0f;    
    public Vector2 CameraHeights = new Vector2(0.666f, 0.3f);

    [Header("Grounding")]
    [Range(0f, 1f)]
    public float FeetDetectorScale = 0.9f;
    [Range(0f, 0.5f)]
    public float FeetDetectorOffset = 0.05f;

    [Header("Jumping")]
    public float JumpVel = 4f;
    [Range(0, 100)]
    public int MaxAirJumps = 1;

    [Header("Leaping")]
    public float LeapVel = 15f;
    public float LeapLiftVel = 9f;

    [Header("Gravity")]
    public Vector3 Gravity = new Vector3(0f, -9.81f, 0f);

    [Header("NoClip")]
    public bool NoClip = false;
    public float NoClipSpeed = 8f;
    public float NoClipBoostSpeed = 15f;

    [Header("Runtime")]
    public bool IsGrounded = false;
    /// <summary>
    /// Is the controller currently moving through the air with no contact or collisions?
    /// </summary>
    public bool IsFlying = false;
    public int JumpsInAir = 0;
    [Range(0f, 1f)]
    public float RealCrouchLerp = 0f;
    public bool CanStand = true;
    public Vector3 CurrentTargetVelocity { get; private set; }
    public Vector3 CurrentRealVelocity { get; private set; }

    [Header("Other")]
    public ExtraDistanceResolveMode DistanceResolveMode = ExtraDistanceResolveMode.Rewind;
    public AnimationCurve SurfaceAngleDistanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float FastResolveDepth = 0.02f;
    public UnityAction OnLeaveGround;
    public UnityAction OnEnterGround;

    [Header("Debug")]
    public bool DrawFeetDetector = true;
    public bool DrawMoveDebug = false;

    private bool lastFrameGrounded = false;

    private CharacterMovementComponent[] comps;

    private void Awake()
    {
        Resolver = new CustomCollisionResolver(Collider);
        Resolver.FastResolveDepth = Resolver.RecommendedFastResolveDepth;
        Resolver.SurfaceAngleDistanceCurve = SurfaceAngleDistanceCurve;

        comps = GetComponents<CharacterMovementComponent>();
        comps = comps.OrderBy((c) => { return c.Order; }).ToArray();
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

            if(lastFrameGrounded != IsGrounded)
            {
                lastFrameGrounded = IsGrounded;
                if (IsGrounded)
                {
                    OnEnterGround?.Invoke();
                }
                else
                {
                    OnLeaveGround?.Invoke();
                }
            }
        }
    }

    private void UpdateCrouching()
    {
        if (!CanStand)
        {
            RealCrouchLerp = 0f;
        }
        else
        {
            RealCrouchLerp = Mathf.MoveTowards(RealCrouchLerp, TargetCrouchLerp, Time.deltaTime * CrouchSpeed);
        }

        float lerp = RealCrouchLerp;
        Collider.height = Mathf.Lerp(RegularHeight, CrouchHeight, lerp);
        Collider.center = Vector3.Lerp(Vector3.zero, new Vector3(0f, (RegularHeight - CrouchHeight) * -0.5f, 0f), lerp);
        HeadYaw.localPosition = new Vector3(0f, Mathf.Lerp(CameraHeights.x, CameraHeights.y, lerp), 0f);
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

        if (IsGrounded)
        {
            TargetCrouchLerp = Input.GetKey(KeyCode.C) ? 1f : 0f;
        }
        else
        {
            TargetCrouchLerp = 0f;
        }

        // Update jumps in air variable.
        if (IsGrounded)
        {
            JumpsInAir = 0;
        }

        if(Collider.isTrigger != ColliderIsTrigger)
            Collider.isTrigger = ColliderIsTrigger;

        Vector3 offset = Vector3.zero;
        CurrentTargetVelocity = Vector3.zero;
        foreach (var c in comps)
        {
            if(c != null && c.enabled)
            {
                var vel = c.MoveUpdate(this);
                offset += vel;
                CurrentTargetVelocity += vel;
            }
        }
        foreach (var c in comps)
        {
            if (c != null && c.enabled)
            {
                var vel = c.MoveLateUpdate(this);
                offset += vel;
                CurrentTargetVelocity += vel;
            }
        }
        offset *= Time.deltaTime;

        if (DrawMoveDebug)
        {
            Debug.DrawLine(transform.position, transform.position + offset, Color.black);
            Debug.DrawLine(transform.position + offset, transform.position + offset + Vector3.up * 0.01f, Color.black);
        }

        var result = Resolver.ResolveFast(transform.position, transform.position + offset);
        if (!result.HasError)
        {
            CurrentRealVelocity = (result.FinalPosition - transform.position) / Time.deltaTime; // Very rough value because of changing delta time.
            transform.position = result.FinalPosition;
            IsFlying = result.NoCollisions;
        }
        else
        {
            Debug.LogWarning(result.Error);
        }

        // Detect the ground...
        UpdateGroundInfo();

        // Detect the head overlapping.
        UpdateHeadInfo();
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
            if (ShouldInteractWithCollider(collider))
            {
                IsGrounded = true;
                break;
            }
        }
    }

    private void UpdateHeadInfo()
    {
        CanStand = true;

        int hits = Physics.OverlapSphereNonAlloc(GetHeadDetectorPosition(), GetHeadDetectorRadius(), Overlaps);
        for (int i = 0; i < hits; i++)
        {
            var collider = Overlaps[i];
            if (ShouldInteractWithCollider(collider))
            {
                CanStand = false;
                break;
            }
        }
    }

    public virtual bool ShouldInteractWithCollider(Collider c)
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

    public Vector3 GetHeadDetectorPosition()
    {
        // TODO make this a capsule overlap instead.
        return transform.TransformPoint(Vector3.up * (RegularHeight * 0.5f - Collider.radius));
    }

    public float GetHeadDetectorRadius()
    {
        return Collider.radius * 0.95f;
    }

    private void OnDrawGizmosSelected()
    {
        if (DrawFeetDetector)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(GetFeetDetectorPosition(), GetFeetDetectorRadius());

            Gizmos.color = CanStand ? Color.green : Color.red;
            Gizmos.DrawWireSphere(GetHeadDetectorPosition(), GetHeadDetectorRadius());
        }
    }
}
