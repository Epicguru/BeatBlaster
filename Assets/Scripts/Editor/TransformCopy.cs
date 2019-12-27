
using UnityEditor;
using UnityEngine;

public static class TransformCopy
{
    private static Vector3 Position;
    private static Vector3 Angles;

    [MenuItem("GameObject/Transform.../Copy", false, 0)]
    public static void Copy()
    {
        var transform = Selection.activeTransform;
        if (transform == null)
            return;

        Position = transform.position;
        Angles = transform.eulerAngles;
    }

    [MenuItem("GameObject/Transform.../Paste", false, 1)]
    public static void Paste()
    {
        var transform = Selection.activeTransform;
        if (transform == null)
            return;

        transform.position = Position;
        transform.eulerAngles = Angles;
    }
}