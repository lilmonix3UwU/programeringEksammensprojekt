using UnityEngine;

public class Gauntlet : MonoBehaviour
{
    [Header("Customize")]
    [SerializeField] private float timeToShake = 0.5f;
    [SerializeField] private float normalDamage = 10f;
    [SerializeField] private float chargedDamage = 20f;
    [SerializeField] private float punchCooldown = 1f;
    private float punchCooldownTime = 0f;
    private float damage;

    [Header("Effects")]
    [SerializeField] private ParticleSystem lightningPunchEffect;
    [SerializeField] private ParticleSystem lightningChargeEffect;

    [Header("Shake")]
    [SerializeField] private ShakeTransformEventData punchShakeData;
    [SerializeField] private ShakeTransformEventData chargeShakeData;
    private float shakeTime;

    private Animator anim;
    private ShakeTransform st;

    private InputManager input;

    private void Start()
    {
        anim = GetComponent<Animator>();
        st = Camera.main.GetComponent<ShakeTransform>();

        damage = normalDamage;

        input = InputManager.Instance;
    }

    private void Update()
    {
        if (input.attack && punchCooldownTime > punchCooldown)
        {
            anim.SetBool("Charging", true);
            
            if (shakeTime >= timeToShake) 
            {
                damage = chargedDamage;
                st.AddShakeEvent(chargeShakeData);
                
                if (!lightningChargeEffect.isPlaying)
                    lightningChargeEffect.Play();
            }
            else 
            {
                damage = Mathf.Lerp(normalDamage, chargedDamage, shakeTime / timeToShake);
                shakeTime += Time.deltaTime;
            }
        }
        if (input.attackUp && punchCooldownTime > punchCooldown)
        {
            anim.SetBool("Charging", false);

            damage = normalDamage;

            st.AddShakeEvent(punchShakeData);
            shakeTime = 0f;

            lightningChargeEffect.Stop();
            lightningPunchEffect.Play();

            punchCooldownTime = 0f;
        }
        else
            punchCooldownTime += Time.deltaTime;
    }
}
