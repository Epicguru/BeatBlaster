
using UnityEngine;

[RequireComponent(typeof(GunController))]
public class ScopeHandler : MonoBehaviour
{
    public GunController Controller
    {
        get
        {
            if (_controller == null)
                _controller = GetComponent<GunController>();
            return _controller;
        }
    }
    private GunController _controller;

    public MeshRenderer ScopeGlassRenderer;
    public Camera ScopeCamera;

    private void Update()
    {
        if(ScopeGlassRenderer != null)
        {
            ScopeGlassRenderer.material.SetFloat("_Visibility", Controller.ADSLerp);
        }

        bool shouldCameraBeActive = Controller.ADSLerp != 0f;
        if(ScopeCamera != null)
        {
            if (ScopeCamera.gameObject.activeSelf != shouldCameraBeActive)
            {
                ScopeCamera.gameObject.SetActive(shouldCameraBeActive);
            }
        }
    }
}