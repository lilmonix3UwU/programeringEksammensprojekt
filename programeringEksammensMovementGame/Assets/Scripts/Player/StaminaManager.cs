using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    public float minStamina = 0.1f;
    public float maxStamina = 0.9f;

    [SerializeField] private Gradient staminaGradient;
    private Image bar;
    private Image barBG;

    private float curStamina;

    public static StaminaManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        bar = transform.Find("Bar").GetComponent<Image>();
        barBG = transform.Find("BarBG").GetComponent<Image>();

        curStamina = maxStamina;
        UpdateStamina();
    }

    public void AddStamina(float amount)
    {
        if (curStamina >= maxStamina)
            return;

        curStamina += amount;
        UpdateStamina();
    }

    public void LoseStamina(float amount)
    {
        if (curStamina <= minStamina)
            return;

        curStamina -= amount;
        UpdateStamina();
    }

    public float GetStamina()
    {
        return curStamina;
    }

    private void UpdateStamina()
    {
        bar.fillAmount = curStamina;

        Color gradientColor = staminaGradient.Evaluate(curStamina);
        Color bgColor = gradientColor * 0.5f;

        bar.color = gradientColor;
        barBG.color = bgColor;
    }
}
