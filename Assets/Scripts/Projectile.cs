using UnityEngine;

[RequireComponent(typeof(PoolObject))]
[RequireComponent(typeof(AutoDestroy))]
public class Projectile : MonoBehaviour
{
    public PoolObject PoolObject
    {
        get
        {
            if (_po == null)
                _po = GetComponent<PoolObject>();
            return _po;
        }
    }
    private PoolObject _po;

    [Header("References")]
    public PoolObject HitEffect;
    public PoolObject HitParticles;

    [Header("Controls")]
    public LayerMask CollisionMask;
    public Vector3 Velocity;
    public float LifeTime = 5f;

    private void Update()
    {       
        Vector3 newPos = transform.position + Velocity * Time.deltaTime;
        Velocity += Physics.gravity * Time.deltaTime;

        if(Physics.Linecast(transform.position, newPos, out RaycastHit hit, CollisionMask))
        {
            PoolObject.Despawn(PoolObject);
            if(HitEffect != null)
            {
                var eff = PoolObject.Spawn(HitEffect);
                eff.transform.position = hit.point + hit.normal * 0.01f;
                eff.transform.forward = -hit.normal;
            }
            if(HitParticles != null)
            {
                var eff = PoolObject.Spawn(HitParticles);
                eff.transform.position = hit.point + hit.normal * 0.01f;
                eff.transform.forward = -hit.normal;
            }

            hit.transform.BroadcastMessage("UponHit", hit.point, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            transform.position = newPos;
        }
    }
}
