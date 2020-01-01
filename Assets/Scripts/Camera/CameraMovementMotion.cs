
using UnityEngine;

public class CameraMovementMotion : MonoBehaviour
{
    public Camera Camera
    {
        get
        {
            if (_cam == null)
                _cam = GetComponentInChildren<Camera>();
            return _cam;
        }
    }
    private Camera _cam;


}
