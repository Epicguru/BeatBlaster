
using System.Collections.Generic;
using UnityEngine;

public class CustomCollisionResolver
{
    // TODO: make sure the CustomCollisionResults are being returned correctly.

    protected static Collider[] Overlaps = new Collider[128];
    protected static RaycastHit[] Hits = new RaycastHit[128];
    private static Dictionary<Collider, float> penetrationDepths = new Dictionary<Collider, float>();

    /// <summary>
    /// Gets or sets the current collider that is used in all method of this class. Should never be null, and should only be of the types Box, Sphere or Capsule.
    /// </summary>
    public Collider Collider { get; set; }
    /// <summary>
    /// When true debug lines are drawn in the editor. Useful to tweak the <see cref="FastResolveDepth"/> value.
    /// </summary>
    public bool DrawDebug { get; set; } = false;
    /// <summary>
    /// The penetration depth used in the <see cref="ResolveFast"/> method. Smaller values produce more accurate collisions at fast speeds but can take much longer to compute.
    /// See <see cref="RecommendedFastResolveDepth"/> for a recommended value, good if you are unsure of a good value.
    /// Larger values are faster to compute but may result in less accurate collisions. As a general rule, the value should be as high as possible but always smaller than the radius of 
    /// the collider. Defaults to 0.1 (10 cm).
    /// </summary>
    public float FastResolveDepth
    {
        get
        {
            return _fastResolveDepth;
        }
        set
        {
            if (value < 0.01f)
                _fastResolveDepth = 0.01f;
            else
                _fastResolveDepth = value;
        }
    }
    /// <summary>
    /// The distance reduction mode used with calculating new path. This ensures that the object will not travel faster than intended.
    /// </summary>
    public ExtraDistanceResolveMode DistanceCheckMode = ExtraDistanceResolveMode.Truncate;
    /// <summary>
    /// The curve used to calculate the distance multiplier based on the initial angle of impact. This affects how far the collider 'slides' along surfaces when it hits them at an angle.
    /// </summary>
    public AnimationCurve SurfaceAngleDistanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    /// <summary>
    /// Gets a useful recommendation for the value of <see cref="FastResolveDepth"/> based on the current collider.
    /// Designed to reduce computation time while still being accurate.
    /// </summary>
    public float RecommendedFastResolveDepth
    {
        get
        {
            if (Collider is CapsuleCollider)
            {
                var cap = Collider as CapsuleCollider;
                return cap.radius * 0.9f;
            }

            if (Collider is BoxCollider)
            {
                var box = Collider as BoxCollider;
                return box.size.magnitude * 0.3f;
            }

            if (Collider is SphereCollider)
            {
                var sphere = Collider as SphereCollider;
                return sphere.radius * 0.9f;
            }

            return 0.2f;
        }
    }

    private float _fastResolveDepth = 0.1f;
    private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    public CustomCollisionResolver(Collider c)
    {
        this.Collider = c;
    }

    /// <summary>
    /// Calculates the final position that the collider should have when moving from <paramref name="startPos"/> to <paramref name="endPos"/>.
    /// This version of the method, compared to <see cref="Resolve(Vector3, Vector3, float, bool)"/>, is optimized for large distances between the start and end position, allowing for fast movement.
    /// Note that the rotation of the collider is taken at the time of calling this method, so adjust collider rotation before calling this.
    /// </summary>
    /// <param name="startPos">The starting position of the collider in world space.</param>
    /// <param name="endPos">The target end position of the collider in world space.</param>
    /// <param name="maxTimeMs">The maximum allowed time to resolve the collisions, in milliseconds. After this time has elapsed then the method automatically quits which may result in incomplete or incorrect collision resolution.
    /// If set to zero or less, infinite time is allowed, but this is dangerous because an infinite loop can be caused in certain situations.</param>
    /// <param name="givePenetrationStats">If set to true, the collision result will include a list of colliders and their penetration depth that were encounted while resolving collisions. Useful for applying forces to them.</param>
    /// <returns>The collision result. Includes final position, collision info and debug stats. Also may contain an error message, see <see cref="CustomCollisionResult.Error"/>.</returns>
    public CustomCollisionResult ResolveFast(Vector3 startPos, Vector3 endPos, float maxTimeMS = 2f, bool givePenetrationStats = false)
    {
        // Sweep along line to check for collision.
        bool didHit = SweepCheck(startPos, endPos, out Vector3 finalPos);
        if (!didHit)
        {
            return new CustomCollisionResult() { FinalPosition = endPos, ComputationItterations = 0, NoCollisions = true };
        }

        // Resolve from the initial point of contact, to just past it. This essentially gives the 'normal' position of the collider, where the collider will not be clipping with the other
        // but this will also prevent the sliding along surfaces that is desired.
        const float IDENTICAL_POS_TOLERANCE = 0.002f * 0.002f; // 2mm
        Vector3 dir = (endPos - startPos).normalized;


        // Note to self: if pen depth is greater than the overall distance that we want to travel (start to end), then due to the nature of the algorithm the resolved position will put the object further than (start to end) distance.
        // Because of this pend depth must be limited to be at most as large as the total distance.
        float depthToUse = _fastResolveDepth;
        //float cap = (endPos - startPos).magnitude;
        //if (depthToUse > cap)
        //    depthToUse = cap;

        Vector3 offset = dir * depthToUse;

        float targetDistance = (endPos - finalPos).magnitude * (1f);
        CustomCollisionResult result = default;
        float multiplier = 1f;

        // To solve the no sliding issue, we slide a bunch of times in very small increments.
        for (int i = 0; i < 1000; i++)
        {
            result = Resolve(finalPos, finalPos + offset, maxTimeMS, givePenetrationStats);

            // If we are moving back to the same position, then we can't move any further.
            if ((finalPos - result.FinalPosition).sqrMagnitude <= IDENTICAL_POS_TOLERANCE)
                break;

            if(i == 0)
            {
                // Work out normal of first surface.
                Vector3 start = finalPos + offset;
                Vector3 end = result.FinalPosition;

                // Work out incoming vector.
                Vector3 start2 = finalPos;
                Vector3 end2 = finalPos + offset;

                // Draw normals.
                if (DrawDebug)
                {
                    Debug.DrawLine(start, end - (end - start) * 0.7f, Color.green);
                    Debug.DrawLine(end2, end2 + (start2 - end2) * 0.3f, Color.green);
                }

                Vector3 normalized1 = (end - start).normalized;
                Vector3 normalized2 = (start2 - end2).normalized;
                float dot = Vector3.Dot(normalized1, normalized2);
                multiplier = Mathf.Clamp01(1f - dot);
                multiplier = SurfaceAngleDistanceCurve.Evaluate(multiplier);
                targetDistance *= multiplier;
            }

            targetDistance -= (finalPos - result.FinalPosition).magnitude;

            // Draw final path (magenta).
            if(DrawDebug)
                Debug.DrawLine(finalPos, result.FinalPosition, new Color(1, 0, 1, 0.7f));

            // Rewind 'jittering' bug fix:
            bool appliedJitterFix = false;
            if(DistanceCheckMode == ExtraDistanceResolveMode.Rewind)
            {
                if(multiplier < 0.01f)
                {
                    DistanceCheckMode = ExtraDistanceResolveMode.Truncate;
                    appliedJitterFix = true;
                }
            }

            bool done = false;
            switch (DistanceCheckMode)
            {
                case ExtraDistanceResolveMode.None:
                    if (targetDistance <= 0f)
                        done = true;
                    break;
                case ExtraDistanceResolveMode.Truncate:
                    if (targetDistance <= 0f && i > 0) // Only applicable after the first move, because otherwise there is nothing to truncate!
                    {
                        // Set this frames final position to last frames final. Essentially rewinds the path by one segment.
                        result.FinalPosition = finalPos;
                        done = true;
                    }
                    break;
                case ExtraDistanceResolveMode.Rewind:
                    if (targetDistance <= 0f)
                    {
                        // Rewind along the 'real' path (magenta).
                        float excess = -targetDistance;
                        Vector3 fixDir = (finalPos - result.FinalPosition);
                        if(excess < fixDir.magnitude)
                        {
                            Vector3 realFinal = result.FinalPosition + fixDir.normalized * excess;
                            result.FinalPosition = realFinal;
                        }
                        else
                        {
                            Vector3 realFinal = finalPos; // Last frame's one.
                            result.FinalPosition = realFinal;
                        }
                        done = true;
                    }
                    break;
            }

            if (appliedJitterFix)
            {
                DistanceCheckMode = ExtraDistanceResolveMode.Rewind;
            }

            if (done)
            {
                break;
            }

            finalPos = result.FinalPosition;
        }
        
        return result;
    }

    public CustomCollisionResult ResolveSimple(Vector3 startPos, Vector3 endPos, float maxTimeMS = 2f, bool givePenetrationStats = false)
    {
        // Sweep along line to check for collision.
        bool didHit = SweepCheck(startPos, endPos, out Vector3 finalPos);
        if (!didHit)
        {
            return new CustomCollisionResult() { FinalPosition = endPos, ComputationItterations = 0, NoCollisions = true };
        }

        float penDepth = FastResolveDepth;
        return Resolve(finalPos, finalPos + (endPos - startPos).normalized * penDepth, maxTimeMS, givePenetrationStats);
    }

    /// <summary>
    /// Calculates the final position that the collider should have when moving from <paramref name="startPos"/> to <paramref name="endPos"/>.
    /// For all movement purposes, <see cref="ResolveFast(Vector3, Vector3, float, bool)"/> should be used instead as it will handle moving at high speeds with more accurate collisions.
    /// Note that the rotation of the collider is taken at the time of calling this method, so adjust collider rotation before calling this.
    /// </summary>
    /// <param name="startPos">The starting position of the collider in world space.</param>
    /// <param name="endPos">The target end position of the collider in world space.</param>
    /// <param name="maxTimeMs">The maximum allowed time to resolve the collisions, in milliseconds. After this time has elapsed then the method automatically quits which may result in incomplete or incorrect collision resolution.
    /// If set to zero or less, infinite time is allowed, but this is dangerous because an infinite loop can be caused in certain situations.</param>
    /// <param name="givePenetrationStats">If set to true, the collision result will include a list of colliders and their penetration depth that were encounted while resolving collisions. Useful for applying forces to them.</param>
    /// <returns>The collision result. Includes final position, collision info and debug stats. Also may contain an error message, see <see cref="CustomCollisionResult.Error"/>.</returns>
    public virtual CustomCollisionResult Resolve(Vector3 startPos, Vector3 endPos, float maxTimeMs = 2f, bool givePenetrationStats = false)
    {
        CustomCollisionResult res = new CustomCollisionResult();
        if(Collider == null)
        {
            res.Error = "Collider is null. Assign a valid collider to CustomCollisionResolver.Collider.";
            return res;
        }

        if(!(Collider is CapsuleCollider) && !(Collider is SphereCollider) && !(Collider is BoxCollider))
        {
            res.Error = $"Collider must be a capsule, a sphere or a box. {Collider.GetType().Name} is not valid.";
            return res;
        }

        if (givePenetrationStats)
        {
            res.ColliderPenetration = new List<(Collider collider, float totalDepth)>();
            penetrationDepths.Clear();
        }

        if (DrawDebug)
        {
            Debug.DrawLine(startPos, endPos, new Color(0, 1, 1, 0.7f));
            Debug.DrawLine(startPos, startPos + (endPos - startPos) * 0.2f, Color.blue);
        }

        watch.Restart();
        Vector3 workingPos = endPos;
        int itterations = 0;
        do
        {
            // Get all the overlaps at the current working position.
            int count = GetAllOverlaps(workingPos, Overlaps);

            // Need to filter out colliders that we don't want to be colliding with...
            int newCount = count;
            for (int i = 0; i < count; i++)
            {
                Collider c = Overlaps[i];
                bool shouldCollide = ShouldCollideWith(c);

                // Remove this collider if it shouldn't be collided with.
                if (!shouldCollide)
                {
                    Overlaps[i] = null;
                    newCount--;
                }
            }

            // If at this working position nothing is overlaping, then we have found the final position.
            if (newCount == 0)
            {
                if (itterations == 0)
                    res.NoCollisions = true;
                break;
            }

            // Take the first collider...
            Collider found = null;
            for (int i = 0; i < count; i++)
            {
                Collider c = Overlaps[i];
                if (c != null)
                {
                    found = c;
                    break;
                }
            }

            // Calculate penetration resolution.
            bool doOverlap = Physics.ComputePenetration(Collider, workingPos, Collider.transform.rotation, found, found.transform.position, found.transform.rotation, out Vector3 resolveDir, out float resolveDst);
            itterations++;

            if (doOverlap)
            {
                // Update stats if necessary.
                if (givePenetrationStats)
                {
                    if(!penetrationDepths.ContainsKey(found))
                        penetrationDepths.Add(found, resolveDst);
                    else
                        penetrationDepths[found] += resolveDst;
                }

                // Move working position out of the way.
                const float ADDITIONAL = 0.0001f;
                Vector3 offset = resolveDir * (resolveDst + ADDITIONAL);
                if (DrawDebug)
                {
                    Debug.DrawLine(workingPos, workingPos + offset, new Color(1f, 0.92f, 0.016f, 0.7f));
                    Debug.DrawLine(workingPos, workingPos + offset * 0.2f, Color.red);
                }
                workingPos += offset;
            }
            else
            {
                //Debug.LogWarning($"Did not overlap with {found.name}!");
                if (DrawDebug)
                {
                    Debug.DrawLine(startPos, endPos, Color.red);
                    Debug.DrawLine(startPos, startPos + Vector3.up * 0.1f, Color.red);
                }                
                break;
            }

        } while (maxTimeMs <= 0f || watch.Elapsed.TotalMilliseconds < maxTimeMs);

        // Stop watch...
        watch.Stop();

        // Write results...
        res.FinalPosition = workingPos;
        res.ComputationTime = (float)watch.Elapsed.TotalSeconds;
        res.ComputationItterations = itterations;
        if (givePenetrationStats)
        {
            foreach (var pair in penetrationDepths)
            {
                res.ColliderPenetration.Add((pair.Key, pair.Value));
            }
        }

        if(maxTimeMs > 0f && watch.Elapsed.TotalMilliseconds >= maxTimeMs)
        {
            res.Error = $"Computation time ({watch.Elapsed.TotalMilliseconds:F2} ms) exceeded the allowed limit ({maxTimeMs:F2} ms). Collisions may not have been resolved correctly!";
        }

        return res;
    }

    /// <summary>
    /// Returns true if this resolver's collider should collide with another collider. Note that this does NOT check for overlap or anything like that.
    /// This is just a way to check, "if these colliders were to overlap, should they interact with each other?".
    /// The default implementation returns false if the other collider is a trigger.
    /// </summary>
    /// <param name="c">The other collider to check against.</param>
    /// <returns>True if the collider should be collided with, false otherwise.</returns>
    public virtual bool ShouldCollideWith(Collider c)
    {
        if (c == null || c == Collider || c.isTrigger)
            return false;
        else
            return true;
    }

    /// <summary>
    /// Performs a collider sweep from the start position to the end one, using the collider's current rotation.
    /// Returns true if anything was hit, and gives the final position of the collider just before it hits the collider in it's path.
    /// </summary>
    /// <param name="start">The starting position, in world space.</param>
    /// <param name="end">The end position, in world space.</param>
    /// <param name="finalPos">Gives the final position of the collider just before it collides with any other collider in it's path between the star and end points. If nothing is collided with, the end position is returned.</param>
    /// <returns>True if any collider is hit along the path, false if nothing is hit.</returns>
    public virtual bool SweepCheck(Vector3 start, Vector3 end, out Vector3 finalPos)
    {
        var box = Collider as BoxCollider;
        var sph = Collider as SphereCollider;
        var cap = Collider as CapsuleCollider;

        Vector3 delta = end - start;
        const float REWIND = 0.01f;

        if(box != null)
        {
            int hitCount = Physics.BoxCastNonAlloc(start, box.size * 0.5f, delta.normalized, Hits, box.transform.rotation, delta.magnitude);

            float minDst = float.MaxValue;
            RaycastHit hit = default;
            bool hasHit = false;
            for (int i = 0; i < hitCount; i++)
            {
                var h = Hits[i];
                if (ShouldCollideWith(h.collider))
                {
                    float dst = h.distance;
                    if (dst < minDst)
                    {
                        minDst = dst;
                        hit = h;
                        hasHit = true;
                    }
                }
            }

            if (!hasHit)
            {
                finalPos = end;
                return false;
            }
            else
            {
                finalPos = start + delta.normalized * (hit.distance - REWIND);
                return true;
            }
        }
        if (sph != null)
        {
            int hitCount = Physics.SphereCastNonAlloc(new Ray(start, delta.normalized), sph.radius, Hits, delta.magnitude);

            float minDst = float.MaxValue;
            RaycastHit hit = default;
            bool hasHit = false;
            for (int i = 0; i < hitCount; i++)
            {
                var h = Hits[i];
                if (ShouldCollideWith(h.collider))
                {
                    float dst = h.distance;
                    if (dst < minDst)
                    {
                        minDst = dst;
                        hit = h;
                        hasHit = true;
                    }
                }
            }

            if (!hasHit)
            {
                finalPos = end;
                return false;
            }
            else
            {
                finalPos = start + delta.normalized * (hit.distance - REWIND);
                return true;
            }
        }
        if (cap != null)
        {
            var trs = cap.transform;
            Vector3 pointA = start + trs.up * (cap.height * 0.5f - cap.radius);
            Vector3 pointB = start - trs.up * (cap.height * 0.5f - cap.radius);
            int hitCount = Physics.CapsuleCastNonAlloc(pointA, pointB, cap.radius, delta.normalized, Hits, delta.magnitude);

            float minDst = float.MaxValue;
            RaycastHit hit = default;
            bool hasHit = false;
            for (int i = 0; i < hitCount; i++)
            {
                var h = Hits[i];
                if (ShouldCollideWith(h.collider))
                {
                    float dst = h.distance;
                    if(dst < minDst)
                    {
                        minDst = dst;
                        hit = h;
                        hasHit = true;
                    }
                }
            }

            if (!hasHit)
            {
                finalPos = end;
                return false;
            }
            else
            {
                finalPos = start + delta.normalized * (hit.distance - REWIND);
                return true;
            }            
        }

        Debug.LogError("Invalid collider type for sweep check!");
        finalPos = end;
        return false;
    }

    /// <summary>
    /// Gets an unfiltered list of colliders that would overlap the collider if placed at a certain world positon, in it's current rotation.
    /// The returned array may contain colliders that should not actually be collided with. You should filter the results using <see cref="ShouldCollideWith(Collider)"/>.
    /// </summary>
    /// <param name="position">The world position that should be checked for overlapping colliders. The rotation of the collider at the time of calling is used.</param>
    /// <param name="overlaps">The array of colliders to populate. Should not be null, and ideally should be large enough to store all the possible overlaps.</param>
    /// <returns>The number of colliders that were overlapped: the amount of items in the array that were actually modified.</returns>
    public virtual int GetAllOverlaps(Vector3 position, Collider[] overlaps)
    {
        var trs = Collider.transform;
        if(Collider is CapsuleCollider)
        {
            var cap = Collider as CapsuleCollider;
            Vector3 pointA = position + trs.up * (cap.height * 0.5f - cap.radius);
            Vector3 pointB = position - trs.up * (cap.height * 0.5f - cap.radius);
            int hits = Physics.OverlapCapsuleNonAlloc(pointA, pointB, cap.radius, overlaps);
            return hits;
        }

        if (Collider is BoxCollider)
        {
            var box = Collider as BoxCollider;
            return Physics.OverlapBoxNonAlloc(position, box.size * 0.5f, overlaps, box.transform.rotation);
        }

        if (Collider is SphereCollider)
        {
            var sphere = Collider as SphereCollider;
            return Physics.OverlapSphereNonAlloc(position, sphere.radius, overlaps);
        }

        return 0;
    }
}

/// <summary>
/// The method used to ensure that the distance moved is not greater than the overall intended linear distance.
/// </summary>
public enum ExtraDistanceResolveMode
{
    /// <summary>
    /// No method is used. Depending on the world geometry, certain moves will travel further than intended.
    /// </summary>
    None,
    /// <summary>
    /// If the path becomes larger than intended, the path will end at the previous segment, which ensures that the distance travelled is always less but not necessarily exactly the same as intended.
    /// </summary>
    Truncate,
    /// <summary>
    /// If the path becomes larger than intended, the last segment in the path is adjusted to match the intended distance exactly. This may cause some issues with very complex or tight world geometry, but will generally work fine.
    /// </summary>
    Rewind
}
