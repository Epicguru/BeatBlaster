
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
public class FallingShell : MonoBehaviour
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
    public Vector3 Vel;
    public float LifeTime = 10f;

    private float timer;

    private void UponSpawn()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > LifeTime)
        {
            Yeet();
            return;
        }

        transform.position += Vel * Time.deltaTime;
        Vel += Physics.gravity * Time.deltaTime;
    }

    private void Yeet()
    {
        PoolObject.Despawn(this.PoolObject);
    }
}
