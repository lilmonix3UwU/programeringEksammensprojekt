using UnityEngine;

[CreateAssetMenu(fileName = "Shake Transform Event", menuName = "Custom/Shake Transform Event", order = 1)]
public class ShakeTransformEventData : ScriptableObject
{
    public enum Target
    {
        Position,
        Rotation
    }

    public Target target = Target.Position;

    public float amplitude = 1;
    public float frequency = 1;

    public float duration = 1;

    public AnimationCurve blendOverLifetime = new AnimationCurve
        (
            new Keyframe(0, 0, Mathf.Deg2Rad * 0, Mathf.Deg2Rad * 720),
            new Keyframe(.2f, 1),
            new Keyframe(1, 0)
        );

    public void Init(float amplitude, float frequency, float duration, AnimationCurve blendOverLifetime, Target target)
    {
        this.target = target;

        this.amplitude = amplitude;
        this.frequency = frequency;

        this.duration = duration;

        this.blendOverLifetime = blendOverLifetime;
    }
}
