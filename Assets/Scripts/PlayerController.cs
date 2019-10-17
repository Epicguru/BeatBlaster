using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController Controller;

    [Header("Movement")]
    public float GravityForce = 9.81f;
    public Vector3 Velocity;
    public bool OnFloor = false;
    public float JumpVelocity = 5f;

    private void Update()
    {
        if(OnFloor && Input.GetKeyDown(KeyCode.Space))
        {
            Velocity.y = JumpVelocity;
        }

        Vector3 gravity = new Vector3(0f, GravityForce, 0f);
        Velocity += gravity * Time.deltaTime;
        var flags = Controller.Move(Velocity * Time.deltaTime);

        OnFloor = false;
        if (flags.HasFlag(CollisionFlags.Below))
        {
            Velocity.y = -0.1f;
            OnFloor = true;
        }
    }
}
