using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float airSpeed = 50f;
    [SerializeField] private float airSpeedPostGrapple = 20f;
    [SerializeField] private float maxAirVel = 50f;
    [SerializeField] private float smoothing = 0.25f;
    [System.NonSerialized] public bool freeze;
    [System.NonSerialized] public Vector3 requestedMove;
    [System.NonSerialized] public Vector3 accel;
    private Rigidbody rb;
    private Grappler grappler;
    private Vector2 smoothInput;
    private Vector2 moveVel;
    private Vector3 lastVel;

    private Transform graphic;
    private Transform cam;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 50f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float groundRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask grappleLayer;
    [System.NonSerialized] public bool grounded = false;
    private Transform groundCheck;
    private bool requestedJump;
    private bool readyToJump = true;
    private bool ungroundedDueToJump;
    private bool wasInAir;
    private float timeSinceUngrounded;
    private float timeSinceJumpRequest;

    [Header("Sounds")]
    [SerializeField] private float footstepInterval = 1f;
    //[SerializeField] private AudioSource[] footstepSounds;
    //[SerializeField] private AudioSource landSound;
    private float footstepTimer = 0f;

    private InputManager input;

    private void Start()
    {
        input = InputManager.Instance;

        cam = Camera.main.transform;

        rb = GetComponent<Rigidbody>();
        grappler = GetComponent<Grappler>();

        graphic = transform.Find("Graphic");
        groundCheck = transform.Find("GroundCheck");
    }

    private void Update()
    {
        // Move input
        Vector2 moveInput = input.move;
        smoothInput = Vector2.SmoothDamp(smoothInput, moveInput, ref moveVel, smoothing);
        requestedMove = graphic.right * smoothInput.x + graphic.forward * smoothInput.y;

        // Rotate
        Vector3 graphicRot = new Vector3(0f, cam.eulerAngles.y, 0f);
        graphic.eulerAngles = graphicRot;

        if (freeze)
            return;

        // Ground check & jump input
        grounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundLayer | grappleLayer);

        bool wasRequestingJump = requestedJump;
        requestedJump = requestedJump || input.jump;
        if (requestedJump && !wasRequestingJump)
            timeSinceJumpRequest = 0f;

        if (grappler.wasGrappling && grounded)
            grappler.wasGrappling = false;

        if (wasInAir && grounded)
        {
            //landSound.Play();
            wasInAir = false;
        }

        // Footsteps
        bool walking = moveInput.sqrMagnitude > 0.01f && grounded;
        if (walking)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                //footstepSounds[Random.Range(0, footstepSounds.Length)].Play();
                footstepTimer = 0;
            }
        }
        else
            footstepTimer = 0;
    }

    private void LateUpdate()
    {
        accel = rb.velocity - lastVel;

        lastVel = rb.velocity;
    }

    private void FixedUpdate()
    {
        if (freeze)
            return;

        // Jump
        bool canCoyoteJump = timeSinceUngrounded < coyoteTime && !ungroundedDueToJump;

        if (requestedJump)
        {
            if (grounded || canCoyoteJump)
            {
                requestedJump = false;
                ungroundedDueToJump = true;
                readyToJump = false;
                wasInAir = true;
                Invoke("ResetJump", jumpCooldown);

                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            }
            else
            {
                timeSinceJumpRequest += Time.deltaTime;

                bool canJumpLater = timeSinceJumpRequest < coyoteTime;
                requestedJump = canJumpLater;
            }
        }

        // Move
        if (grounded)
        {
            timeSinceUngrounded = 0f;

            if (readyToJump)
                ungroundedDueToJump = false;

            rb.velocity = new Vector3(requestedMove.x * moveSpeed, rb.velocity.y, requestedMove.z * moveSpeed);
        }
        else
        {
            timeSinceUngrounded += Time.deltaTime;

            float effectiveAirSpeed = grappler.wasGrappling ? airSpeedPostGrapple : airSpeed;
            rb.AddForce(requestedMove * effectiveAirSpeed);

            if (grappler.wasGrappling)
                return;

            Vector3 clampedVel = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0f, rb.velocity.z), maxAirVel);
            rb.velocity = new Vector3(clampedVel.x, rb.velocity.y, clampedVel.z);
        }
    }

    private void ResetJump() => readyToJump = true;
}
