
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

    public float LifeTime = 10;

    private float timer = 0f;

    private void UponSpawn()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= LifeTime)
        {
            PoolObject.Despawn(PoolObject);
            return;
        }
    }
}
