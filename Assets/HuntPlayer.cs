using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HuntPlayer : MonoBehaviour
{
    public Transform ToHunt;
    public NavMeshAgent Agent;

    private void Update()
    {
        Agent.SetDestination(ToHunt.position);    
    }
}
