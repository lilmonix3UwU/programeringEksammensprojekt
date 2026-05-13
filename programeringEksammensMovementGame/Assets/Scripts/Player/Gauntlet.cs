using UnityEngine;

public class Gauntlet : MonoBehaviour
{
    [Header("Customize")]
    [SerializeField] private float timeToShake = 0.5f;
    [SerializeField] private float punchCooldown = 1f;
    private float punchCooldownTime = 0f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem lightningPunchEffect;
    [SerializeField] private ParticleSystem lightningChargeEffect;

    [Header("Shake")]
    [SerializeField] private ShakeTransformEventData punchShakeData;
    [SerializeField] private ShakeTransformEventData chargeShakeData;
    private float shakeTime;

    private Animator anim;
    private ShakeTransform st;

    private bool colliding;
    private EnemyDeath enemyDeath;

    private InputManager input;

    private void Start()
    {
        anim = GetComponent<Animator>();
        st = Camera.main.GetComponent<ShakeTransform>();

        input = InputManager.Instance;
    }

    private void Update()
    {
        if (input.attack && punchCooldownTime > punchCooldown)
        {
            anim.SetBool("Charging", true);
            
            if (shakeTime >= timeToShake) 
            {
                st.AddShakeEvent(chargeShakeData);
                
                if (!lightningChargeEffect.isPlaying)
                    lightningChargeEffect.Play();
            }
            else 
            {
                shakeTime += Time.deltaTime;
            }
        }
        if (input.attackUp && punchCooldownTime > punchCooldown)
        {
            anim.SetBool("Charging", false);

            st.AddShakeEvent(punchShakeData);
            shakeTime = 0f;

            lightningChargeEffect.Stop();
            lightningPunchEffect.Play();

            punchCooldownTime = 0f;
            
            if (colliding && enemyDeath != null)
                enemyDeath.DIE();
        }
        else
            punchCooldownTime += Time.deltaTime;
    }
    
    private void OnTriggerStay(Collider other) 
    {
        if (other.TryGetComponent(out EnemyDeath death)) 
        {
            colliding = true;
            enemyDeath = death;
        }
    }
    
    private void OnTriggerExit(Collider other) 
    {
        if (other.GetComponent<EnemyDeath>()) 
            colliding = false;
    }
}
