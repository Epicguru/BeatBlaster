
using UnityEngine;

public class ScopeCamera : MonoBehaviour
{
    public GunController GunController
    {
        get
        {
            if (_gc == null)
                _gc = GetComponentInParent<GunController>();
            return _gc;
        }
    }
    private GunController _gc;

    public float ZeroDistance = 0f;
    public float ZeroHeight = 0f;

    private void Update()
    {
        UpdateAngle();
    }

    private void UpdateAngle()
    {
        float s = GunController.MuzzleVelocity;
        float s2 = s * s;
        float s4 = s2 * s2;
        float g = Physics.gravity.y;
        float x = ZeroDistance;
        float x2 = x * x;
        float y = -ZeroHeight;

        float root = s4 - g * (g * x2 + 2 * y * s2);

        if (root <= 0f)
        {
            Debug.LogWarning($"Cannot range for {ZeroDistance} with {ZeroHeight} height using the current muzzle velocity of {s}. Target will always be out of range.");
            return;
        }

        float angle = Mathf.Atan2(s2 - Mathf.Sqrt(root), g * x) * Mathf.Rad2Deg;

        transform.localEulerAngles = new Vector3(180f - (angle - 90f), 0f, 0f);
    }
}