using UnityEngine;

public class Grappler : MonoBehaviour
{
    //[SerializeField] private GameObject hookInstance;
    //[SerializeField] GameObject hookItem;
    //private GameObject hook;
    private Camera cam;
    private PlayerMove plrMove;
    private Rigidbody rb;

    [Header("Grappling")]
    [SerializeField] private float maxGrappleDist = 25f;
    [SerializeField] private LayerMask grappleLayer;
    [System.NonSerialized] public bool wasGrappling;
    private Vector3 grapplePoint;
    private bool grappling;
    private bool grapplingMiss;

    [Header("Control")]
    [SerializeField] private float thrustForce = 2f;
    [SerializeField] private float climbSpeed = 1f;

    [Header("Joint")]
    [SerializeField] private float jointSpring = 4.5f;
    [SerializeField] private float jointDamper = 7f;
    [SerializeField] private float jointMassScale = 4.5f;
    [SerializeField] private float jointPullAmount = 5f;
    private SpringJoint joint;
    private float jointLength;

    [Header("Prediction")]
    [SerializeField] private float predictionSphereRadius = 5f;
    [SerializeField] private float missStopGrappleTime = 0.5f;
    private bool miss = true;

    [Header("Cooldown")]
    [SerializeField] private float grappleCooldown = 3f;
    [SerializeField] private float hookDownAmount = 1f;
    //private Vector3 initialHookPos;
    //private Vector3 hookDownPos;
    private float grappleCooldownTimer;
    private bool canGrapple;

    [Header("Stamina")]
    [SerializeField] private float passiveStaminaLoss = 0.1f;
    [SerializeField] private float passiveStaminaGain = 0.2f;
    [SerializeField] private float staminaAirMultiplier = 0.25f;

    [Header("Rope")]
    [SerializeField] private float minSnapDist = 0.1f;
    [SerializeField] private float attachSpeed = 12f;
    [SerializeField] private float velocityMult = 0.25f;
    [SerializeField] private int quality = 500;
    [SerializeField] private float damper = 14f;
    [SerializeField] private float strength = 800f;
    [SerializeField] private float velocity = 15f;
    [SerializeField] private float waveCount = 3f;
    [SerializeField] private float waveHeight = 1f;
    [SerializeField] private AnimationCurve affectCurve;
    private LineRenderer lr;
    private Vector3 curGrapplePos;
    private Vector3 grappleTo;
    private RopeSpring spring;

    [Header("Sounds")]
    //[SerializeField] private AudioSource throwSound;
    //[SerializeField] private AudioSource hitSound;

    private InputManager input;
    private StaminaManager staminaMgr;
    private CrosshairManager crosshairMgr;

    private void Start()
    {
        input = InputManager.Instance;
        staminaMgr = StaminaManager.Instance;
        crosshairMgr = CrosshairManager.Instance;

        plrMove = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;

        lr = GetComponentInChildren<LineRenderer>();

        //hook = Instantiate(hookInstance);
        //hook.SetActive(false);

        //initialHookPos = hookItem.transform.localPosition;
        //hookDownPos = initialHookPos + Vector3.down * hookDownAmount;

        spring = new RopeSpring();
        spring.SetTarget(0);

        curGrapplePos = lr.transform.position;
    }

    private void Update()
    {
        bool hasStamina = staminaMgr.GetStamina() > staminaMgr.minStamina;
        bool noStamina = staminaMgr.GetStamina() <= staminaMgr.minStamina;

        if (input.grappleDown && canGrapple && hasStamina)
            StartGrapple();
        if (input.grappleUp || noStamina)
            StopGrapple();

        grappling = joint != null;

        if (!canGrapple && !grappling && !grapplingMiss)
        {
            grappleCooldownTimer += Time.deltaTime;

            //hookItem.transform.localPosition = Vector3.Lerp(hookDownPos, initialHookPos, grappleCooldownTimer / grappleCooldown);

            if (grappleCooldownTimer >= grappleCooldown)
            {
                //hookItem.transform.localPosition = initialHookPos;
                grappleCooldownTimer = 0;
                canGrapple = true;
            }
        }

        if ((!grappling || (!grapplingMiss && !grappling)) && canGrapple)
        {
            CheckForGrapplePoints();

            float staminaGain = plrMove.grounded ? passiveStaminaGain : passiveStaminaGain * staminaAirMultiplier;
            staminaMgr.AddStamina(staminaGain * Time.deltaTime);
        }
        else if (grappling)
        {
            GrappleControl();

            staminaMgr.LoseStamina(passiveStaminaLoss * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        grappleTo = grappling || grapplingMiss ? grapplePoint : lr.transform.position;

        float hookDistFromPoint = Vector3.Distance(curGrapplePos, grappleTo);

        if (!grappling && hookDistFromPoint <= minSnapDist)
            DestroyRope();
        else if (grappling || (!grappling && hookDistFromPoint > minSnapDist) || grapplingMiss)
            DrawRope();
    }

    private void StartGrapple()
    {
        //hook.SetActive(true);
        //hookItem.SetActive(false);

        Vector3 difference = grapplePoint - lr.transform.position;

        //hook.transform.rotation = Quaternion.LookRotation(difference);

        //throwSound.Play();

        if (miss)
        {
            Invoke("StopGrapple", missStopGrappleTime);

            grapplingMiss = true;
            canGrapple = false;
            return;
        }

        //hitSound.Play();

        plrMove.freeze = true;
        plrMove.grounded = false;

        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        float distFromGrapplePoint = difference.magnitude;
        jointLength = distFromGrapplePoint > jointPullAmount ? distFromGrapplePoint - jointPullAmount : distFromGrapplePoint;

        joint.maxDistance = jointLength;
        joint.minDistance = 0;

        joint.spring = jointSpring;
        joint.damper = jointDamper;
        joint.massScale = jointMassScale;

        crosshairMgr.ChangeCrosshair(CrosshairType.Normal);

        canGrapple = false;
    }

    private void StopGrapple()
    {
        if (miss)
        {
            grapplingMiss = false;
            return;
        }

        wasGrappling = true;
        plrMove.freeze = false;

        Destroy(joint);
    }

    private void GrappleControl()
    {
        // Swinging
        rb.AddForce(plrMove.requestedMove * thrustForce);

        // Climbing Upwards
        if (input.climbingUpwards)
        {
            if (jointLength > 0)
                jointLength -= climbSpeed * Time.deltaTime;

            joint.maxDistance = jointLength;
        }

        // Climbing Downwards
        if (input.climbingDownwards)
        {
            if (jointLength < maxGrappleDist)
                jointLength += climbSpeed * Time.deltaTime;

            joint.maxDistance = jointLength;
        }
    }

    private void CheckForGrapplePoints()
    {
        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.transform.position, predictionSphereRadius, cam.transform.forward, out sphereCastHit, maxGrappleDist, grappleLayer);

        RaycastHit raycastHit;
        Physics.Raycast(cam.transform.position, cam.transform.forward, out raycastHit, maxGrappleDist, grappleLayer);

        // Option 1 - Direct hit
        if (raycastHit.point != Vector3.zero)
        {
            miss = false;

            grapplePoint = raycastHit.point;
            crosshairMgr.ChangeCrosshair(CrosshairType.Grapple);
        }

        // Option 2 - Indirect (predicted) hit
        else if (sphereCastHit.point != Vector3.zero)
        {
            miss = false;

            grapplePoint = sphereCastHit.point;
            crosshairMgr.ChangeCrosshair(CrosshairType.Grapple);
        }

        // Option 3 - Miss
        else
        {
            miss = true;

            grapplePoint = cam.transform.position + cam.transform.forward * maxGrappleDist;
            crosshairMgr.ChangeCrosshair(CrosshairType.Normal);
        }
    }

    private void DrawRope()
    {
        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.UpdateSpring();

        Vector3 up = Quaternion.LookRotation((grapplePoint - lr.transform.position).normalized) * Vector3.up;

        //if (!grappling || (!grappling && !grapplingMiss))
            //hook.transform.rotation = Quaternion.LookRotation(curGrapplePos - lr.transform.position);

        float velAmplifier = grapplingMiss ? 1f : 1f + rb.velocity.magnitude * velocityMult;
        curGrapplePos = Vector3.Lerp(curGrapplePos, grappleTo, Time.deltaTime * attachSpeed * velAmplifier);

        //hook.transform.position = curGrapplePos;

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta) * Time.deltaTime;

            lr.SetPosition(i, Vector3.Lerp(lr.transform.position, curGrapplePos, delta) + offset);
        }
    }

    private void DestroyRope()
    {
        SnapHook();

        //hook.SetActive(false);
        //hookItem.SetActive(true);

        spring.Reset();

        if (lr.positionCount > 0)
            lr.positionCount = 0;
    }

    public void SnapHook()
    {
        curGrapplePos = lr.transform.position;
    }

    public bool IsGrappling() => grappling;
}
