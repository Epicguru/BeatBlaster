
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Intended as a drop-in replacement for the standard CharacterController, which lacks key features such as variable gravity or player tilt.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
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
    public Rigidbody Body
    {
        get
        {
            if (_body == null)
                _body = GetComponent<Rigidbody>();
            return _body;
        }
    }
    private Rigidbody _body;

    [Header("References")]
    public Transform Head;
    public Transform HeadYaw;

    [Header("Movement Settings")]
    public float BaseSpeed = 8f;
    public float RunSpeed = 14f;

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
    public float AirDrag = 0.05f, GroundDrag = 5f;

    [Header("Jumping")]
    [Range(0, 100)]
    public int MaxAirJumps = 1;

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
    public int JumpsInAir = 0;
    [Range(0f, 1f)]
    public float RealCrouchLerp = 0f;
    public bool CanStand = true;
    public Vector3 CurrentRealVelocity { get; private set; }

    public bool IsRunning { get; set; }
    public bool IsCrouching { get; private set; }

    public UnityAction OnLeaveGround;
    public UnityAction OnEnterGround;

    [Header("Debug")]
    public bool DrawFeetDetector = true;

    private CharacterMovementComponent[] comps;
    [NonSerialized]
    internal Dictionary<Type, CharacterMovementComponent> compsDict = new Dictionary<Type, CharacterMovementComponent>();

    private void Awake()
    {
        comps = GetComponents<CharacterMovementComponent>();
        comps = comps.OrderBy((c) => { return c.Order; }).ToArray();
        foreach (var comp in comps)
        {
            compsDict.Add(comp.GetType(), comp);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            NoClip = !NoClip;

        if (IsGrounded)
        {
            TargetCrouchLerp = Input.GetKey(KeyCode.C) ? 1f : 0f;
        }
        else
        {
            TargetCrouchLerp = 0f;
        }

        foreach (var c in comps)
        {
            if (c != null && c.enabled)
            {
                c.MoveUpdate(this);
            }
        }
    }

    private void FixedUpdate()
    {
        if (NoClip)
        {
            MoveNoClip();
        }
        else
        {
            UpdateCrouching();
            MoveRegular();
            UpdateGroundInfo();
            UpdateHeadInfo();
            UpdateDrag();
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
            RealCrouchLerp = Mathf.MoveTowards(RealCrouchLerp, TargetCrouchLerp, Time.fixedDeltaTime * CrouchSpeed);
        }

        float lerp = RealCrouchLerp;
        Collider.height = Mathf.Lerp(RegularHeight, CrouchHeight, lerp);
        Collider.center = Vector3.Lerp(Vector3.zero, new Vector3(0f, (RegularHeight - CrouchHeight) * -0.5f, 0f), lerp);
        HeadYaw.localPosition = new Vector3(0f, Mathf.Lerp(CameraHeights.x, CameraHeights.y, lerp), 0f);
    }

    private void UpdateDrag()
    {
        if (IsGrounded)
            Body.drag = GroundDrag;
        else
            Body.drag = AirDrag;
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
        Body.MoveRotation(Quaternion.RotateTowards(transform.localRotation, target, angleSpeed * Time.fixedDeltaTime));

        // Update jumps in air variable.
        if (IsGrounded)
        {
            JumpsInAir = 0;
        }

        Vector3 offset = Vector3.zero;
        foreach (var c in comps)
        {
            if (c != null && c.enabled)
            {
                var vel = c.MoveFixedUpdate(this);
                offset += vel;
                Debug.DrawLine(Body.position, Body.position + vel * 0.1f, c.DebugColor);
            }
        }
        offset *= Time.fixedDeltaTime;

        Vector3 oldPos = Body.position;
        Vector3 targetPos = Body.position + offset;
        Body.MovePosition(targetPos);
        CurrentRealVelocity = (Body.position - oldPos) / Time.fixedDeltaTime; // Should give fairly accurate velocity due to fixed delta time.
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
        bool old = IsGrounded;
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

        if (old != IsGrounded)
        {
            if (IsGrounded)
            {
                OnEnterGround?.Invoke();
            }
            else
            {
                OnLeaveGround?.Invoke();
                Body.velocity = CurrentRealVelocity;
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

    public float GetDirectionalVelocity(Vector3 axis)
    {
        var worldVel = Body.velocity;
        var localVel = HeadYaw.InverseTransformVector(worldVel);

        Vector3 axisThing = new Vector3(localVel.x * axis.x, localVel.y * axis.y, localVel.z * axis.z);

        return axisThing.magnitude;
    }

    public void SetDirectionalVelocity(Vector3 localAxis)
    {
        Vector3 world = HeadYaw.TransformVector(localAxis);
        Vector3 newVel = Body.velocity;

        const float DEADZONE = 0.01f;
        if(Mathf.Abs(world.x) > DEADZONE)
        {
            newVel.x = world.x;
        }
        if (Mathf.Abs(world.y) > DEADZONE)
        {
            newVel.y = world.y;
        }
        if (Mathf.Abs(world.z) > DEADZONE)
        {
            newVel.z = world.z;
        }

        Body.velocity = newVel;
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
