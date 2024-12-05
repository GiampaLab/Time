namespace Time.AnimationConfig;
using Time.Components;
public class AnimationUtils
{
    public static double ArmStateToDegree(ArmState state)
    {
        switch (state)
        {
            case ArmState.Zero:
                return 0.0;
            case ArmState.Three:
                return 90.0;
            case ArmState.Six:
                return 180.0;
            case ArmState.Nine:
                return 270.0;
            case ArmState.None:
                return 225.0;
            case ArmState.HPOne:
                return 45.0;
            default: return 0;
        }
    }
}
