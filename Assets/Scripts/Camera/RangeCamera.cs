
using UnityEngine;

public class RangeCamera : MonoBehaviour
{
    public Camera Cam;

    public Vector3 FollowOffset;
    public float FollowFOV = 50f;
    public Vector3 HitOffset;
    public float HitFOV = 40f;

    private void Update()
    {
        var p = Projectile.LastFiredProjectile;

        if(p != null)
        {
            transform.position = p.transform.position + FollowOffset;
            transform.LookAt(p.transform);
            Cam.fieldOfView = FollowFOV;
        }
        else
        {
            transform.position = Projectile.LastHit + HitOffset;
            transform.LookAt(Projectile.LastHit);
            Cam.fieldOfView = HitFOV;
        }
    }
}
