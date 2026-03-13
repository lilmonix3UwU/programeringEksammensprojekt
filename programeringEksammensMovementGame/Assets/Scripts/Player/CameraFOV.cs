using UnityEngine;

public class CameraFOV : MonoBehaviour
{
    [SerializeField] private float fovMult = 5f;
    [SerializeField] private float minFov = 60f;
    [SerializeField] private float maxFov = 100f;
    [SerializeField] private float fovSmoothing = 0.15f;
    [SerializeField] private float minVel = 25f;

    private Rigidbody rb;

    private float fovVel;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        rb = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        float dynamicFov = minFov - minVel + new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude * fovMult;
        float smoothedDynamicFov = Mathf.SmoothDamp(cam.fieldOfView, dynamicFov, ref fovVel, fovSmoothing);
        cam.fieldOfView = Mathf.Clamp(smoothedDynamicFov, minFov, maxFov);
    }
}
