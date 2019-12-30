using UnityEngine;

public class test : MonoBehaviour
{
    public FreeFlyMovement Mover;

    private void OnDrawGizmos()
    {
        if(Mover.CollisionResolver != null)
        {
            Mover.CollisionResolver.SweepCheck(transform.position, transform.position + transform.forward * 5f, out Vector3 dest);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(dest, 0.3f);
        }
    }
}
