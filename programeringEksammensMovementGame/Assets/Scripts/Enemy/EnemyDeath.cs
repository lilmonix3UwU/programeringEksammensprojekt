using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDeath : MonoBehaviour
{
    [SerializeField] HeadNShooting headNShooting;
    [SerializeField] GameObject bum;
    EnemyNavigation enemyNavigation;

    private void Start() 
    {
        enemyNavigation = transform.parent.Find("body").GetComponent<EnemyNavigation>();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            DIE();
        }
    }

    public void DIE()
    {
        enemyNavigation.enabled = false;
        enemyNavigation.gameObject.GetComponent<NavMeshAgent>().enabled = false;
        headNShooting.enabled = false;
        enemyNavigation.gameObject.AddComponent<Rigidbody>();
        headNShooting.gameObject.AddComponent<Rigidbody>();
        StartCoroutine(BOOMDIE());
    }
    private IEnumerator BOOMDIE()
    {
        yield return new WaitForSeconds(2);
        WinManager.Instance.enemies--;
        GameObject temp1 = Instantiate(bum, enemyNavigation.transform.position, bum.transform.rotation);
        GameObject temp2 = Instantiate(bum, headNShooting.transform.position, bum.transform.rotation);
        Destroy(temp1, 1);
        Destroy(temp2, 1);
        Destroy(transform.parent.gameObject);
    }
}
