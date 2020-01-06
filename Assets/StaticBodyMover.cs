using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StaticBodyMover : MonoBehaviour
{
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

    [Tooltip("The target angular velocity, measured in degrees per second.")]
    public Vector3 TargetAngularVelocity;

    private void FixedUpdate()
    {
        Body.AddRelativeTorque(TargetAngularVelocity);
    }
}
