namespace Time.AnimationConfig;
using Time.Components;
public class AnimationUtils
{
    public static object MapAnimationConfig(AnimationConfig config)
    {
        return new
        {
            state = config.State,
            elementReference = config.ElementReference,
            easing = config.EasingFunction,
            direction = Enum.GetName(typeof(Direction), config.Direction),
            duration = config.Duration,
            delay = config.Delay,
            amplitude = config.Amplitude
        };
    }
    public static int ArmStateToDegree(ArmState state)
    {
        return state switch
        {
            ArmState.Zero => 0,
            ArmState.Three => 90,
            ArmState.Six => 180,
            ArmState.Nine => 270,
            ArmState.None => 225,
            ArmState.HPOne => 45,
            _ => 0,
        };
    }
}
