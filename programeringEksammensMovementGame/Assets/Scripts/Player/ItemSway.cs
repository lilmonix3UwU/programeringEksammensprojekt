using UnityEngine;

public class ItemSway : MonoBehaviour
{
    [SerializeField] private CameraLook camLook;

    [SerializeField] private float amountMult;
    [SerializeField] private float maxAmount;
    [SerializeField] private float smoothAmount;

    private Vector3 initialPos;

    private InputManager input;

    private void Start()
    {
        input = InputManager.Instance;

        initialPos = transform.localPosition;
    }

    private void Update()
    {
        // Get the camera x and y axises and time them with the desired amount and then clamp them
        float xMov = -input.look.x * amountMult * camLook.sens;
        float yMov = -input.look.y * amountMult * camLook.sens;
        xMov = Mathf.Clamp(xMov, -maxAmount, maxAmount);
        yMov = Mathf.Clamp(yMov, -maxAmount, maxAmount);

        // Smoothly lerp to the new position
        Vector3 pos = new Vector3(xMov, yMov, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, pos + initialPos, 1f - Mathf.Exp(-smoothAmount * Time.deltaTime));
    }
}