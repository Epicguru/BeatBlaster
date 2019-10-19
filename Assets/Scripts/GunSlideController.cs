using UnityEngine;

[ExecuteInEditMode]
public class GunSlideController : MonoBehaviour
{
    public Transform Target;
    public Vector3 ClosedPos, OpenPos;
    [Range(0f, 1.5f)]
    public float OpenLerp = 0f;

    [Header("Code editing only!")]
    public bool OverrideLock = false;
    public bool LockOpen = false;

    private void LateUpdate()
    {
        Target.localPosition = Vector3.LerpUnclamped(ClosedPos, OpenPos, (LockOpen && !OverrideLock) ? 1f : OpenLerp);
    }
}
