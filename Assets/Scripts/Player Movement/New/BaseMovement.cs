
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

    [Header("Acceleration")]
    public float MaxAccelerationForce = 1000f;
    public float MaxDecelerationForce = 3000f;

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
    public bool IsGrounded = false;
    public Vector3 CurrentMoveVel;

    [Header("Debug")]
    public bool DrawFeetDetector = true;

    private Vector3 lastPos;

    private void Update()
    {
        UpdateThrowing();
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

        Vector3 currentVel = Vector3.zero;
        Vector3 inputRaw = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
            inputRaw.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            inputRaw.x += 1f;
        if (Input.GetKey(KeyCode.S))
            inputRaw.z -= 1f;
        if (Input.GetKey(KeyCode.W))
            inputRaw.z += 1f;
        inputRaw.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? RunSpeed : BaseSpeed;

        Vector3 final = Vector3.zero;
        final += HeadYaw.forward * inputRaw.z * speed;
        final += HeadYaw.right * inputRaw.x * speed;

        currentVel += final;
        if (IsGrounded)
        {
            Body.MovePosition(Body.position + currentVel * Time.fixedDeltaTime);
        }
        else
        {
            Body.velocity += Physics.gravity * Time.fixedDeltaTime;
        }

        CurrentMoveVel = (Body.position - lastPos) / Time.fixedDeltaTime;
        lastPos = Body.position;

        // Detect the ground...
        UpdateGroundInfo();
    }

    private void OnLeaveGround()
    {
        Body.drag = 0.05f;
        Body.velocity = CurrentMoveVel;
        Debug.Log($"Left ground with vel {CurrentMoveVel} ({CurrentMoveVel.magnitude})");
    }

    private void OnEnterGround()
    {
        Body.drag = 5f;
        //Body.velocity = Vector3.zero;
    }

    private void AddForce(Vector3 force, Color c)
    {
        Body.AddForce(force);
        DrawForceLine(force * 0.01f, c);
    }

    private void DrawForceLine(Vector3 line, Color c)
    {
        Debug.DrawLine(transform.position, transform.position + line, c);
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
        bool old = IsGrounded;
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

        if(old != IsGrounded)
        {
            if (IsGrounded)
            {
                OnEnterGround();
            }
            else
            {
                OnLeaveGround();
            }
        }
    }

    public virtual bool DoesColliderCountForGrounded(Collider c)
    {
        if (c == null || c == this.Collider || c.isTrigger)
            return false;

        return true;
    }

    public Rigidbody GrabbedObject;
    private void UpdateThrowing()
    {
        if (Input.GetKeyDown(KeyCode.Mouse4))
        {
            bool didHit = Physics.Raycast(new Ray(Head.position, Head.forward), out RaycastHit hit, 15f);
            if (didHit)
            {
                GrabbedObject = hit.rigidbody;
            }
        }

        if(GrabbedObject != null)
        {
            Vector3 targetPos = Head.position + Head.forward * 2f;
            GrabbedObject.velocity = ((targetPos - GrabbedObject.position) * 15f).ClampToMagnitude(20f);
        }

        if (Input.GetKeyUp(KeyCode.Mouse4))
        {
            if(GrabbedObject != null)
            {
                GrabbedObject.velocity = Head.forward * 30f;
                GrabbedObject = null;
            }
        }
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