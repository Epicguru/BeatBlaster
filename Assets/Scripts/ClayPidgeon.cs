
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PoolObject))]
public class ClayPidgeon : MonoBehaviour
{
    public Rigidbody Body
    {
        get
        {
            if (_body == null)
                _body = GetComponent<Rigidbody>();
            return _body;
        }
    }
    private Rigidbody _body;

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

    private void UponSpawn()
    {
        GetComponent<Renderer>().material.color = Color.green;
    }

    public void OnCollisionEnter(Collision collision)
    {
        GetComponent<Renderer>().material.color = Color.red;
    }

    private void UponProjectileHit(ProjectileHit hit)
    {
        PoolObject.Despawn(this.PoolObject);
        Debug.Log("Nice shot!");
    }
}