
using System;
using UnityEngine;

public abstract class CharacterMovementComponent : MonoBehaviour
{
    public CustomCharacterController CCC
    {
        get
        {
            if (_ccc == null)
                _ccc = GetComponent<CustomCharacterController>();
            return _ccc;
        }
    }
    private CustomCharacterController _ccc;

    public int Order = 0;
    public Color DebugColor = Color.green;

    /// <summary>
    /// Used to collect input and that kind of stuff.
    /// </summary>
    /// <param name="c"></param>
    public virtual void MoveUpdate(CustomCharacterController c)
    {

    }

    /// <summary>
    /// Should return any desired velocity.
    /// You should forces/velocity when in air for accurate control. See <see cref="CustomCharacterController.IsGrounded"/>
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public virtual Vector3 MoveFixedUpdate(CustomCharacterController c)
    {
        return Vector3.zero;
    }

    public T GetMoveComp<T>() where T : CharacterMovementComponent
    {
        var d = CCC.compsDict;
        Type t = typeof(T);
        if (d.ContainsKey(t))
            return d[t] as T;
        else
            return null;
    }
}
