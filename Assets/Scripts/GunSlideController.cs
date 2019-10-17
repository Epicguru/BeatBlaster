using UnityEngine;

[ExecuteInEditMode]
public class GunSlideController : MonoBehaviour
{
    public Transform Target;
    public Vector3 ClosedPos, OpenPos;
    [Range(0f, 1.5f)]
    public float OpenLerp = 0f;

    [Header("Code editing only!")]
    [Range(0f, 1.5f)]
    public float OverrideLerp = 0f;

    public bool AnimDriven = true;

    private void LateUpdate()
    {
        Target.localPosition = Vector3.LerpUnclamped(ClosedPos, OpenPos, AnimDriven ? OpenLerp : OverrideLerp);
    }
}
