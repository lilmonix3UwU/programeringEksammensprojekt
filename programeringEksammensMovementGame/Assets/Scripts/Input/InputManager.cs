using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private InputActions inputActions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        inputActions = new InputActions();
    }

    private void OnEnable() => inputActions.Enable();

    private void OnDisable() => inputActions.Disable();

    // Player actions
    public Vector2 move => inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 look => inputActions.Player.Look.ReadValue<Vector2>();

    public bool jump => inputActions.Player.Jump.WasPressedThisFrame();
    public bool jumpSustain => inputActions.Player.Jump.IsPressed();

    public bool crouch => inputActions.Player.Crouch.IsPressed();

    public bool grappleDown => inputActions.Player.Grapple.WasPressedThisFrame();
    public bool grappleUp => inputActions.Player.Grapple.WasReleasedThisFrame();

    public bool climbingUpwards => inputActions.Player.Jump.IsPressed();
    public bool climbingDownwards => inputActions.Player.Crouch.IsPressed();

    public bool pause => inputActions.Player.Pause.WasPressedThisFrame();
}
