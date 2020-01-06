using UnityEngine;

[RequireComponent(typeof(PoolObject))]
[RequireComponent(typeof(AutoDestroy))]
public class Projectile : MonoBehaviour
{
    public static Projectile LastFiredProjectile;
    public static Vector3 LastHit;

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
    public float HitImpulse = 2.5f;

    [Header("Damage")]
    public int Damage = 10;

    private void UponDespawn()
    {
        GetComponent<TrailRenderer>().Clear();
        if (LastFiredProjectile == this)
            LastFiredProjectile = null;
    }

    private void UponSpawn()
    {
        LastFiredProjectile = this;
    }

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
                eff.transform.parent = hit.collider.transform;
            }
            if(HitParticles != null)
            {
                var eff = PoolObject.Spawn(HitParticles);
                eff.transform.position = hit.point + hit.normal * 0.01f;
                eff.transform.forward = -hit.normal;
            }

            var phit = new ProjectileHit()
            {
                WorldPoint = hit.point,
                WorldNormal = hit.normal,
                Collider = hit.collider,
                IncomingVelocity = Velocity,
                HealthChange = -Damage
            };

            LastHit = hit.point;

            hit.transform.SendMessageUpwards("UponProjectileHit", phit, SendMessageOptions.DontRequireReceiver);

            if(hit.collider.attachedRigidbody != null)
            {
                hit.collider.attachedRigidbody.AddForceAtPosition(Velocity.normalized * HitImpulse, hit.point, ForceMode.Impulse);
            }
        }
        else
        {
            transform.position = newPos;
        }
    }
}
