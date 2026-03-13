using UnityEngine;
using UnityEngine.UI;

public enum CrosshairType
{
    Normal,
    Grapple
}

public class CrosshairManager : MonoBehaviour
{
    [SerializeField] private Sprite normalCrosshair;
    [SerializeField] private Sprite grappleCrosshair;

    private Image img;

    public static CrosshairManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        img = GetComponent<Image>();
    }

    public void ChangeCrosshair(CrosshairType type)
    {
        switch (type)
        {
            case CrosshairType.Normal:
                img.sprite = normalCrosshair;
                break;
            case CrosshairType.Grapple:
                img.sprite = grappleCrosshair;
                break;
        }
    }
}
