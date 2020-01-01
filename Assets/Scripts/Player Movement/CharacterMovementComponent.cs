
using UnityEngine;

public abstract class CharacterMovementComponent : MonoBehaviour
{
    public int Order = 0;
    public abstract Vector3 MoveUpdate(CustomCharacterController controller);
    public virtual Vector3 MoveLateUpdate(CustomCharacterController controller) { return Vector3.zero; }
}
