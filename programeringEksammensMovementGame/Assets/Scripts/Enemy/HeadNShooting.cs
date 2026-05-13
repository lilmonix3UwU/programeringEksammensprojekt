using UnityEngine;

public class HeadNShooting : MonoBehaviour
{
    [SerializeField] GameObject lazur;
    [SerializeField] float viewAngle = 70;
    [SerializeField] float maxRotationSpeed = 30;
    [SerializeField] float shotSpeed = 30;
    [SerializeField] float shotCooldown = 1;
    float currentShotCooldown = 0;

    GameObject player;
    EnemyNavigation enemyNavigation;



    void Start()
    {
        enemyNavigation = transform.parent.Find("body").GetComponent<EnemyNavigation>();
    
        player = FindFirstObjectByType<PlayerMove>().gameObject;
        currentShotCooldown = shotCooldown;
    }

    void Update()
    {
        transform.position = new Vector3(enemyNavigation.transform.position.x, enemyNavigation.transform.position.y + 1.3f, enemyNavigation.transform.position.z);
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


        if (!enemyNavigation.playerVisible && !enemyNavigation.hunting)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, enemyNavigation.transform.rotation, maxRotationSpeed * Time.deltaTime);
            if (currentShotCooldown != shotCooldown)
            {
                currentShotCooldown = shotCooldown;
            }
        }
        else
        {

            if (enemyNavigation.playerVisible)
            {
                Quaternion r = Quaternion.LookRotation(player.transform.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, r, maxRotationSpeed * Time.deltaTime);
                if (Vector3.Distance(player.transform.position, transform.position) < enemyNavigation.shootingRange && Vector3.Distance(player.transform.position, transform.position) > enemyNavigation.shootingRangeMin)
                {
                    if (currentShotCooldown > 0)
                    {
                        currentShotCooldown -= Time.deltaTime;
                    }
                    else
                    {
                        GameObject temp = Instantiate(lazur, transform.position, transform.rotation);
                        temp.GetComponent<Lazur>().Launch(towardsPlayer.normalized, shotSpeed);
                        Destroy(temp, 4);
                        currentShotCooldown = shotCooldown;
                    }


                }
                else
                {
                    if (currentShotCooldown != shotCooldown)
                    {
                        currentShotCooldown = shotCooldown;
                    }
                }
            }
            else
            {
                Quaternion r = Quaternion.LookRotation(enemyNavigation.lastKnownPlayerLocation - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, r, maxRotationSpeed * Time.deltaTime);
                if (currentShotCooldown != shotCooldown)
                {
                    currentShotCooldown = shotCooldown;
                }
            }

        }



    }
}
