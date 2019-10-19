using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class GenericAnimationCallback : MonoBehaviour
{
    public void Event(AnimationEvent e)
    {
        SendMessageUpwards("UponAnimationEvent", e, SendMessageOptions.DontRequireReceiver);
    }
}