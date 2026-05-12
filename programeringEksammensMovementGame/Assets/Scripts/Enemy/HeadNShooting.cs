using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadNShooting : MonoBehaviour
{
    [SerializeField] EnemyNavigation enemyNavigation;
    [SerializeField] float viewAngle = 70;
    [SerializeField] float maxRotationSpeed = 30;



    GameObject player;



    void Start()
    {
        player = enemyNavigation.player;
    }

    void Update()
    {
        transform.position = new Vector3(enemyNavigation.transform.position.x, enemyNavigation.transform.position.y + 0.65f, enemyNavigation.transform.position.z);
        Vector3 towardsPlayer = player.transform.position - transform.position;
        RaycastHit hit;
        if (Vector3.Angle(transform.forward, towardsPlayer) < viewAngle && Physics.Raycast(transform.position, towardsPlayer, out hit, enemyNavigation.agroRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                enemyNavigation.playerVisible = true;
            }
            else
            {
                enemyNavigation.playerVisible = false;
            }
        }
        else
        {
            enemyNavigation.playerVisible = false;
        }


        if (!enemyNavigation.playerVisible)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, enemyNavigation.transform.rotation, maxRotationSpeed * Time.deltaTime);
        }
        else
        {
            Quaternion r = Quaternion.LookRotation(player.transform.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, r, maxRotationSpeed * Time.deltaTime);


        }



    }
}
