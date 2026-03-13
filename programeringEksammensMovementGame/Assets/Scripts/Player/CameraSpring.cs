using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    [Min(0.01f)] [SerializeField] private float halfLife = 0.075f;
    [SerializeField] private float freq = 18f;
    [SerializeField] private float angularDisplace = 2f;

    private Vector3 springPos;
    private Vector3 springVel;

    private void Start()
    {
        springPos = transform.position;
        springVel = Vector3.zero;
    }

    private void Update()
    {
        Spring(ref springPos, ref springVel, transform.position, halfLife, freq, Time.deltaTime);

        Vector3 relativeSpringPos = springPos - transform.position;
        float springHeight = Vector3.Dot(relativeSpringPos, Vector3.up);

        transform.localEulerAngles = new Vector3(-springHeight * angularDisplace, 0f, 0f);
    }

    private static void Spring(ref Vector3 value, ref Vector3 vel, Vector3 target, float halfLife, float freq, float timeStep)
    {
        float dampingRatio = -Mathf.Log(0.5f) / (freq * halfLife);
        float dampingFactor = 1.0f + 2.0f * timeStep * dampingRatio * freq;
        float freqSquared = freq * freq;
        float timeStepFreqSquared = timeStep * freqSquared;
        float timeStepSquaredFreqSquared = timeStep * timeStepFreqSquared;
        float detInv = 1.0f / (dampingFactor + timeStepSquaredFreqSquared);

        Vector3 valueTerm = dampingFactor * value + timeStep * vel + timeStepSquaredFreqSquared * target;
        Vector3 velTerm = vel + timeStepFreqSquared * (target - value);

        value = valueTerm * detInv;
        vel = velTerm * detInv;

    }
}
