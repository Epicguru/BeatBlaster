using UnityEngine;
using UnityEngine.AI;

public class CharacterRagdoll : MonoBehaviour
{
    public Rigidbody[] Bodies;
    public Animator Anim;
    public NavMeshAgent NavMeshAgent;

    private void Awake()
    {
        GetComponent<Health>().UponHealthChange += OnHealthChange;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            EnableRagdoll();
        }
    }

    public void EnableRagdoll()
    {
        NavMeshAgent.enabled = false;
        Anim.enabled = false;
        foreach (var body in Bodies)
        {
            if (body != null && body.isKinematic)
            {
                body.isKinematic = false;
                body.velocity = Vector3.zero;
            }                
        }
    }

    public void EnableRagdoll(Rigidbody body, Vector3 point, Vector3 impulse)
    {
        EnableRagdoll();
        body?.AddForceAtPosition(impulse, point, ForceMode.Impulse);
    }

    private void OnHealthChange(Health h, int change, ProjectileHit hit)
    {
        if (h.IsDead)
        {
            EnableRagdoll();
            //EnableRagdoll(hit.Collider.attachedRigidbody, hit.WorldPoint, hit.IncomingVelocity.normalized * 200f);
        }
    }
}
