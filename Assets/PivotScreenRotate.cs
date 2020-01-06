using UnityEngine;

public class PivotScreenRotate : MonoBehaviour
{
    public Transform Target;
    public Vector3 TargetOffset;
    public float MaxSpeed = 180f;
    public float MaxSpeedAngle = 90f;

    private void Update()
    {
        if (Target == null)
            return;

        Quaternion targetRot = Quaternion.LookRotation((Target.position + TargetOffset) - transform.position, Vector3.up);
        float delta = Quaternion.Angle(transform.rotation, targetRot);
        float speed = Mathf.Clamp01(delta / MaxSpeedAngle) * MaxSpeed;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, speed);
    }
}
