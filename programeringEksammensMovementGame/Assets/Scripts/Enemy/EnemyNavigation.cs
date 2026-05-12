using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    [SerializeField] HeadNShooting headNShooting;

 
    [SerializeField] float wanderRange = 10.0f;
    [SerializeField] float wanderPauseMin = 1.0f;
    [SerializeField] float wanderPauseMax = 10.0f;
    public float shootingRange = 20.0f;
    public float shootingRangeMin = 5.0f;
    public float agroRange = 30.0f;
    public GameObject player;
    [SerializeField] float agroTimer;
    public bool hunting = false;
    public Vector3 lastKnownPlayerLocation;

    public bool playerVisible = false;
    public bool playerTooClose = false;
    
    
    NavMeshAgent navMeshAgent;
    Vector3 wanderPoint;
    float wanderPause = 2;


    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(transform.position);
        player = FindFirstObjectByType<PlayerMove>().gameObject;
    }

    void Update()
    {
        if (playerVisible)
        {

            lastKnownPlayerLocation = player.transform.position;
            agroTimer = 5;
            hunting = true;

        }
        else if ( agroTimer > 0)
        {
            agroTimer -= Time.deltaTime;
        }

        if (agroTimer <= 0 && hunting)
        {
            hunting = false;
        }

        if (playerTooClose && shootingRangeMin < Vector3.Distance(player.transform.position, transform.position))
        {
            playerTooClose = false;
        }
        else if (!playerTooClose && shootingRangeMin > Vector3.Distance(player.transform.position, transform.position))
        {
            playerTooClose = true;
        }

        if (hunting)
        {
            if (!playerVisible)
            {
                if (Vector3.Distance(transform.position, navMeshAgent.destination) < 2 || Physics.Raycast(navMeshAgent.destination, lastKnownPlayerLocation - navMeshAgent.destination, Vector3.Distance(lastKnownPlayerLocation, navMeshAgent.destination), 0))
                {
                    Vector3 trialPos = (Random.insideUnitSphere * 0.8f);
                    NavMeshHit trialPosNavMesh;
                    trialPos = new Vector3(trialPos.x + 0.2f, trialPos.y + 0.2f, trialPos.z + 0.2f) * 20;
                    trialPos = player.transform.position + trialPos;
                    if (NavMesh.SamplePosition(trialPos, out trialPosNavMesh, 20, NavMesh.AllAreas))
                    {
                        Debug.DrawRay(trialPosNavMesh.position, lastKnownPlayerLocation - trialPosNavMesh.position);
                        if (!Physics.Raycast(trialPosNavMesh.position, lastKnownPlayerLocation - trialPosNavMesh.position, Vector3.Distance(lastKnownPlayerLocation, trialPosNavMesh.position), 0))
                        {
                            navMeshAgent.SetDestination(trialPosNavMesh.position);
                        }
                    }
                }

            }
            else if (playerTooClose)
            {
                Vector3 trialPos = Vector3.zero;
                trialPos = transform.position - player.transform.position;
                trialPos = (new Vector3(trialPos.x, transform.position.y, trialPos.z)).normalized;
                trialPos = (trialPos * (10 - Vector3.Distance(player.transform.position, transform.position))) + transform.position;
                NavMeshHit trialPosNavMesh;
                if (NavMesh.SamplePosition(trialPos, out trialPosNavMesh, 10, NavMesh.AllAreas))
                {
                    navMeshAgent.SetDestination(trialPosNavMesh.position);
                }

            }
            else if (Vector3.Distance(player.transform.position, transform.position) > shootingRange)
            {
                Vector3 d = (player.transform.position - transform.position).normalized * (shootingRange - Vector3.Distance(player.transform.position, transform.position));
                d = d + transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(-d, out hit, agroRange, NavMesh.AllAreas))
                {
                    navMeshAgent.SetDestination(hit.position);
                }
            }
            else if (navMeshAgent.destination != transform.position)
            {

                navMeshAgent.SetDestination(transform.position);
            }
        }
        else if (Vector3.Distance(transform.position, navMeshAgent.destination) < 2)
        {
            if (wanderPause > 0)
            {
                wanderPause -= Time.deltaTime;
            }
            else
            {
                Vector3 trialPos = Random.insideUnitSphere * wanderRange;
                NavMeshHit trialPosNavMesh;
                if (NavMesh.SamplePosition(trialPos, out trialPosNavMesh, wanderRange, NavMesh.AllAreas))
                {
                    navMeshAgent.SetDestination(trialPosNavMesh.position);
                    wanderPause = Random.Range(wanderPauseMin, wanderPauseMax);
                }
            }

        }
    }
}
