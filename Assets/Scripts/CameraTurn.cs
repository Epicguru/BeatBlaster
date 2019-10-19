using UnityEngine;

public class CameraTurn : MonoBehaviour
{
    public float Sensitvity = 0.5f;

    [Header("References")]
    public Transform Horizontal;
    public Transform Vertical;

    private float verticalAngle;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float cX = Input.GetAxisRaw("Mouse X");
        float cY = Input.GetAxisRaw("Mouse Y");

        Horizontal.Rotate(0f, cX * Sensitvity, 0f, Space.Self);
        verticalAngle -= cY * Sensitvity;
        verticalAngle = Mathf.Clamp(verticalAngle, -90f, 90f);
        Vertical.localEulerAngles = new Vector3(verticalAngle, 0f, 0f);
    }
}
