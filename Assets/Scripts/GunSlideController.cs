using UnityEngine;

[ExecuteInEditMode]
public class GunSlideController : MonoBehaviour
{
    public Transform Target;

    [Header("Position")]
    public Vector3 ClosedPos;
    public Vector3 OpenPos;

    [Header("Rotation")]
    public bool UseRotation = false;
    public Vector3 ClosedRotation, OpenRotation;

    [Range(0f, 1.5f)]
    public float OpenLerp = 0f;

    [Header("Code editing only!")]
    public bool OverrideLock = false;
    public bool LockOpen = false;

    private void LateUpdate()
    {
        Target.localPosition = Vector3.LerpUnclamped(ClosedPos, OpenPos, (LockOpen && !OverrideLock) ? 1f : OpenLerp);

        if (UseRotation)
        {
            Target.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(ClosedRotation), Quaternion.Euler(OpenRotation), (LockOpen && !OverrideLock) ? 1f : OpenLerp);
        }
    }
}
