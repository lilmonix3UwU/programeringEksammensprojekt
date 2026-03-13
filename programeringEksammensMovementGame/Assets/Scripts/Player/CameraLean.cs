using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;
    [SerializeField] private float strength = 0.1f;
    [SerializeField] private float strengthResponse = 5f;

    private PlayerMove plrMove;

    private Vector3 dampedAccel;
    private Vector3 dampedAccelVel;

    private float smoothStrength;

    private void Start()
    {
        smoothStrength = strength;

        plrMove = GetComponentInParent<PlayerMove>();
    }

    private void Update()
    {
        Vector3 planarAccel = Vector3.ProjectOnPlane(plrMove.accel, Vector3.up);
        float damping = planarAccel.magnitude > dampedAccel.magnitude ? attackDamping : decayDamping;

        dampedAccel = Vector3.SmoothDamp(dampedAccel, planarAccel, ref dampedAccelVel, damping, float.PositiveInfinity, Time.deltaTime);

        // Get the rotation axis based on the acceleration vector
        Vector3 leanAxis = Vector3.Cross(dampedAccel.normalized, Vector3.up).normalized;

        // Reset rotation to that of its parent
        transform.localRotation = Quaternion.identity;

        // Rotate around the lean axis
        // For some reason the lean axis leans the opposite direction of the damped acceleration, so we just invert it
        smoothStrength = Mathf.Lerp(smoothStrength, strength, 1f - Mathf.Exp(-strengthResponse * Time.deltaTime));

        transform.rotation = Quaternion.AngleAxis(dampedAccel.magnitude * smoothStrength, -leanAxis) * transform.rotation;
    }
}
