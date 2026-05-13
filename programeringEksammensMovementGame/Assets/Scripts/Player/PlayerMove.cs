using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float airSpeed = 20f;
    [SerializeField] private float windThreshold = 20f;
    [SerializeField] private float windEffectVelMult = 0.8f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;
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

    private Animator gauntletAnim;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 50f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float groundRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [System.NonSerialized] public bool grounded = false;
    private Transform groundCheck;
    private bool requestedJump;
    private bool readyToJump = true;
    private bool ungroundedDueToJump;
    private bool wasInAir;
    private float timeSinceUngrounded;
    private float timeSinceJumpRequest;

    [Header("Wall Run")]
    [SerializeField] private float wallDist = 0.5f;
    [SerializeField] private float minWallRunHeight = 1.5f;
    [SerializeField] private float wallRunGravity = 10f;
    [SerializeField] private float wallRunJumpForceUp = 2000f;
    [SerializeField] private float wallRunJumpForceSide = 1000f;
    [SerializeField] private float camTilt = 5f;
    [SerializeField] private float camTiltSmooth = 2f;
    public float tilt { get; private set; }
    private float tiltVel;
    private bool canWallRun;
    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    [Header("Dash")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashCooldown = 1f;
    private float dashCooldownTime = 0f;
    private bool requestedDash;

    [Header("Effects")]
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem windEffect;

    [Header("Shake")]
    [SerializeField] private ShakeTransformEventData dashShakeData;
    private ShakeTransform st;

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

        gauntletAnim = FindFirstObjectByType<Gauntlet>().GetComponent<Animator>();

        st = cam.GetComponent<ShakeTransform>();
    }

    private void Update()
    {
        // Move input
        requestedMove = graphic.right * input.move.x + graphic.forward * input.move.y;

        // Rotate
        graphic.eulerAngles = new Vector3(0f, cam.eulerAngles.y, 0f);

        // Dash input
        requestedDash = (requestedDash || input.dash) && dashCooldownTime > dashCooldown;
        dashCooldownTime += Time.deltaTime;

        // Wind effect
        if (rb.velocity.magnitude > windThreshold)
        {
            ParticleSystem.MainModule windEffectMain = windEffect.main;
            windEffectMain.startSpeed = rb.velocity.magnitude * windEffectVelMult;

            if (!windEffect.isPlaying)
                windEffect.Play();
        }
        else
        {
            if (windEffect.isPlaying)
                windEffect.Stop();
        }

        rb.drag = grounded ? groundDrag : airDrag;

        if (freeze)
            return;

        // Ground check & jump input
        grounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundLayer);

        bool wasRequestingJump = requestedJump;
        requestedJump = requestedJump || input.jump;
        if (requestedJump && !wasRequestingJump)
            timeSinceJumpRequest = 0f;

        if (grappler.wasGrappling && grounded)
            grappler.wasGrappling = false;

        if (wasInAir && grounded)
            wasInAir = false;

        // Wall run
        canWallRun = !Physics.Raycast(transform.position, Vector3.down, minWallRunHeight);
        wallLeft = Physics.Raycast(transform.position, -cam.right, out leftWallHit, wallDist);
        wallRight = Physics.Raycast(transform.position, cam.right, out rightWallHit, wallDist);

        // Anim
        bool walking = input.move.sqrMagnitude > 0.01f && grounded;
        gauntletAnim.SetBool("Walking", walking);
    }

    private void LateUpdate()
    {
        accel = rb.velocity - lastVel;

        lastVel = rb.velocity;
    }

    private void FixedUpdate()
    {
        // Dash
        if (requestedDash)
        {
            requestedDash = false;
            dashCooldownTime = 0f;

            dashEffect.Play();
            rb.AddForce(cam.transform.forward * dashForce, ForceMode.Impulse);
            st.AddShakeEvent(dashShakeData);
        }

        if (freeze)
            return;
            
        // Wall run
        if (canWallRun) 
        {
            if (wallLeft)
                StartWallRun();
            else if (wallRight)
                StartWallRun();
            else
                StopWallRun();
        }
        else
            StopWallRun();

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

            rb.AddForce(requestedMove * moveSpeed, ForceMode.Acceleration);
        }
        else
        {
            timeSinceUngrounded += Time.deltaTime;

            rb.AddForce(requestedMove * airSpeed, ForceMode.Acceleration);
        }
    }
    
    private void StartWallRun() 
    {
        rb.useGravity = false;

        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        float tiltAngle = wallLeft ? -camTilt : camTilt;
        tilt = Mathf.SmoothDamp(tilt, tiltAngle, ref tiltVel, camTiltSmooth);

        if (requestedJump) 
        {
            requestedJump = false;

            Vector3 wallRunJumpLeft = transform.up * wallRunJumpForceUp + leftWallHit.normal * wallRunJumpForceSide;
            Vector3 wallRunJumpRight = transform.up * wallRunJumpForceUp + rightWallHit.normal * wallRunJumpForceSide;
            Vector3 wallRunJumpDir = wallLeft ? wallRunJumpLeft : wallRunJumpRight;
            
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(wallRunJumpDir, ForceMode.Impulse);
        }
    }
    
    private void StopWallRun() 
    {
        rb.useGravity = true;
        tilt = Mathf.SmoothDamp(tilt, 0f, ref tiltVel, camTiltSmooth);
    }

    private void ResetJump() => readyToJump = true;
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag == "Hurt")
            SceneManager.LoadScene(0);
    }
}
