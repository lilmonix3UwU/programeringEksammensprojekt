using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public float sens = 0.1f;

    private Vector3 eulerAngles;

    private InputManager input;

    private void Start()
    {
        input = InputManager.Instance;

        Cursor.lockState = CursorLockMode.Locked;

        transform.eulerAngles = eulerAngles;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        eulerAngles += new Vector3(-input.look.y, input.look.x) * sens;
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, -90f, 90f);
        transform.eulerAngles = eulerAngles;
    }
}
