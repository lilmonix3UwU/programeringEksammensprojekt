using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{


 
    [SerializeField] float minWanderRange = 2.0f;
    [SerializeField] float maxWanderRange = 10.0f;
    [SerializeField] float agroRange = 30.0f;

    public bool playerVisible = false;

    [SerializeField] bool hunting = false;
    Vector3 lastKnownPlayerLocation;
    NavMeshAgent navMeshAgent;


    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {


        if (hunting)
        {
            if (!playerVisible)
            {
                // do what you made in diagram with circles and trial n errorr
            }
            else if (navMeshAgent.destination != transform.position)
            {
                navMeshAgent.SetDestination(transform.position);
            }
        }
        else
        {
            //make it wander to random point

        }
    }
}
