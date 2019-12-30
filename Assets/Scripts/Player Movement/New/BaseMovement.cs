
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BaseMovement : MonoBehaviour
{
    private static Collider[] Overlaps = new Collider[64];

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

    [Header("References")]
    public Transform Head;
    public Transform HeadYaw;

    [Header("Movement Settings")]
    public float BaseSpeed = 8f;
    public float RunSpeed = 14f;
    public float JumpVel = 4f;

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

    [Header("Debug")]
    public bool DrawFeetDetector = true;

    private void Awake()
    {
        Body.mass = 0f;
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.N))
            NoClip = !NoClip;

        if (NoClip)
        {
            MoveNoClip();
        }
        else
        {
            MoveRegular();
        }
    }

    private void MoveRegular()
    {
        Collider.enabled = true;

        // Face downwards towards gravity.
        transform.up = Gravity.sqrMagnitude < 0.001f ? Vector3.up : -Gravity;

        // Update forces vel with current gravity;
        if (!IsGrounded)
            GravityVelocity += Gravity * Time.fixedDeltaTime;
        else
            GravityVelocity = Gravity.normalized * 0.5f;

        if (!Collider.enabled)
            Collider.enabled = true;

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

        Vector3 totalVel = worldDir * speed + GravityVelocity + ForcesVelocity;

        Body.velocity = totalVel;

        // Detect the ground...
        UpdateGroundInfo();
    }

    private void MoveNoClip()
    {
        if(Collider.enabled)
            Collider.enabled = false;

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
        Vector3 offset = direction.normalized * speed * Time.fixedDeltaTime;

        transform.position += offset;
    }

    public void AddJumpVelocity(float length)
    {
        GravityVelocity = -Gravity.normalized * length;
    }

    public Vector3 GetFeetDetectorPosition()
    {
        return Body.position - Body.transform.up * (Collider.height * 0.5f - Collider.radius + FeetDetectorOffset);
    }

    public float GetFeetDetectorRadius()
    {
        return Collider.radius * FeetDetectorScale;
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

    private void OnDrawGizmos()
    {
        if (DrawFeetDetector)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(GetFeetDetectorPosition(), GetFeetDetectorRadius());
        }        
    }
}