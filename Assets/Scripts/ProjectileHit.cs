
using UnityEngine;

public struct ProjectileHit
{
    public Vector3 WorldPoint;
    public Vector3 WorldNormal;
    public Collider Collider;
    public Vector3 IncomingVelocity;
    public int HealthChange;
}
