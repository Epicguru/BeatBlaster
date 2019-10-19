
using UnityEngine;

public class ClayPidgeonSpawner : MonoBehaviour
{
    public ClayPidgeon Prefab;

    public Vector3 Direction;
    public Vector3 SpawnOffset;
    public Vector2 Speed;
    public int BurstCount = 1;
    public Vector2 BurstInterval = new Vector2(1f, 1.5f);

    public bool IsFiring { get; private set; }

    private int remainingToFire = 0;
    private float timer = 0f;
    private float toWait = 0f;

    public Vector3 WorldDirection
    {
        get
        {
            return transform.TransformDirection(Direction);
        }
    }
    public Vector3 WorldSpawn
    {
        get
        {
            return transform.TransformPoint(SpawnOffset);
        }
    }

    private void UponHit()
    {
        Fire();
    }

    public void Fire()
    {
        if (IsFiring)
            return;

        IsFiring = true;
        remainingToFire = BurstCount;
        timer = 0f;
        toWait = Random.Range(BurstInterval.x, BurstInterval.y);
    }

    private void Update()
    {
        if (IsFiring)
        {
            timer += Time.deltaTime;
            if(timer >= toWait)
            {
                toWait = Random.Range(BurstInterval.x, BurstInterval.y);
                timer = 0f;

                remainingToFire--;
                if (remainingToFire == 0)
                    IsFiring = false;

                var spawned = PoolObject.Spawn(Prefab);
                spawned.transform.position = WorldSpawn;
                spawned.Body.velocity = WorldDirection * Random.Range(Speed.x, Speed.y);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(WorldSpawn, 0.2f);
        Gizmos.DrawLine(WorldSpawn, WorldSpawn + WorldDirection);
    }
}