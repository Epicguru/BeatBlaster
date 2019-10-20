
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
public class AutoDestroy : MonoBehaviour
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
    public ParticleSystem ParticleSystem
    {
        get
        {
            if (_ps == null)
                _ps = GetComponent<ParticleSystem>();
            return _ps;
        }
    }
    private ParticleSystem _ps;

    public float LifeTime = 10;

    private float timer = 0f;

    private void UponSpawn()
    {
        timer = 0f;
        if(ParticleSystem != null)
        {
            ParticleSystem.Play(true);
        }
    }

    private void UponDespawn()
    {
        // This may seem odd, but there is a reason for it. Trust me.
        PoolObject.Despawn(this.PoolObject);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= LifeTime)
        {
            PoolObject.Despawn(PoolObject);
            if(ParticleSystem != null)
                ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            return;
        }
    }
}
