
using System.Collections.Generic;
using UnityEngine;

public struct CustomCollisionResult
{
    /// <summary>
    /// If true then there was an error during resolution that means that the results are invalid.
    /// </summary>
    public bool HasError { get { return Error != null; } }

    /// <summary>
    /// An error message. May be null if no error occured. Check <see cref="HasError"/>.
    /// </summary>
    public string Error;

    /// <summary>
    /// The final position that was calculated. This is where the collider should move to.
    /// </summary>
    public Vector3 FinalPosition;

    /// <summary>
    /// True if moving to the new position created no collisions at all.
    /// </summary>
    public bool NoCollisions;

    /// <summary>
    /// The list that contains colliders and their corresponding penetration depth during the move. This can be used, for example, to add forces to those bodies.
    /// This field may be null unless <see cref="CustomCollisionResolver.Resolve(Vector3, Vector3, float, bool)"/>'s givePenetrationStats is set to true.
    /// </summary>
    public List<(Collider collider, float totalDepth)> ColliderPenetration;

    /// <summary>
    /// The time it took to resolve the move, in seconds.
    /// </summary>
    public float ComputationTime;
    /// <summary>
    /// The number of itterations it took to resolve the move. Each itteration requires overlaps to be calculated, and for penetration to be resolved.
    /// </summary>
    public int ComputationItterations;
}