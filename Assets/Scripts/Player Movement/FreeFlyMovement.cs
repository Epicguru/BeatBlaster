using UnityEngine;

/// <summary>
/// A character/camera controller that allows the user to fly around the scene freely, optionally without collision resolution (noclip).
/// </summary>
[RequireComponent(typeof(Collider))]
public class FreeFlyMovement : MonoBehaviour
{
    public Collider Collider
    {
        get
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();
            return _collider;
        }
    }
    private Collider _collider;

    [Header("Settings")]
    public bool NoClip = false;
    public float BaseSpeed = 10f;
    public float BoostedSpeed = 25f;

    [Header("Collision Resolving")]
    public float PenDepth = 0.2f;
    public ExtraDistanceResolveMode DstResolveMode = ExtraDistanceResolveMode.None;

    [Header("Mouse Look")]
    public bool EnableMouseLook = false;
    public bool CaptureMouse = true;
    public float MouseSensitivity = 1f;

    [Header("Debug")]
    public bool DrawDebug = false;
    public bool ForwardSpeedTest = false;

    public CustomCollisionResolver CollisionResolver;

    private float horizontalLook;
    private float verticalLook;

    private void Awake()
    {
        CollisionResolver = new CustomCollisionResolver(Collider);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            NoClip = !NoClip;

        CollisionResolver.DrawDebug = DrawDebug;
        CollisionResolver.FastResolveDepth = PenDepth;
        CollisionResolver.DistanceCheckMode = DstResolveMode;

        if (EnableMouseLook)
            UpdateMouseLook();

        UpdateMovement();
    }

    private void UpdateMouseLook()
    {
        horizontalLook += Input.GetAxisRaw("Mouse X") * MouseSensitivity;
        verticalLook -= Input.GetAxisRaw("Mouse Y") * MouseSensitivity;
        verticalLook = Mathf.Clamp(verticalLook, -90f, 90f);

        transform.localEulerAngles = new Vector3(verticalLook, horizontalLook, 0f);

        if (CaptureMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateMovement()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            direction -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += transform.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction -= transform.forward;
        }
        if (ForwardSpeedTest || Input.GetKey(KeyCode.W))
        {
            direction += transform.forward;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction -= transform.up;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += transform.up;
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? BoostedSpeed : BaseSpeed;
        Vector3 offset = direction.normalized * speed * Time.deltaTime;

        //if (offset == Vector3.zero)
        //    return;

        if (NoClip)
        {
            if (Collider.enabled)
                Collider.enabled = false;

            transform.position += offset;
        }
        else
        {
            if (!Collider.enabled)
                Collider.enabled = true;

            if(DrawDebug)
                Debug.DrawLine(transform.position, transform.position + offset, Color.black);
            var res = CollisionResolver.ResolveFast(transform.position, transform.position + offset);
            if (!res.HasError)
            {
                transform.position = res.FinalPosition;
            }
            else
            {
                Debug.LogWarning($"Movement error: {res.Error} (Itter: {res.ComputationItterations})");
            }
        }
    }
}
