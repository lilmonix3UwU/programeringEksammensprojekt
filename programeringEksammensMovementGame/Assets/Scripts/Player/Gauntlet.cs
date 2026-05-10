using UnityEngine;

public class Gauntlet : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem lightningEffect;

    [Header("Shake")]
    [SerializeField] private ShakeTransformEventData punchShakeData;

    private Animator anim;
    private ShakeTransform st;

    private InputManager input;

    private void Start()
    {
        anim = GetComponent<Animator>();
        st = Camera.main.GetComponent<ShakeTransform>();

        input = InputManager.Instance;
    }

    private void Update()
    {
        if (input.attack)
        {
            anim.SetTrigger("Punch");
            st.AddShakeEvent(punchShakeData);
            lightningEffect.Play();
        }
    }
}
