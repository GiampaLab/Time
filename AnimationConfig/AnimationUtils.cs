namespace Time.AnimationConfig;
using Time.Components;
public class AnimationUtils
{
    public static int ArmStateToDegree(ArmState state)
    {
        switch (state)
        {
            case ArmState.Zero:
                return 0;
            case ArmState.Three:
                return 90;
            case ArmState.Six:
                return 180;
            case ArmState.Nine:
                return 270;
            case ArmState.None:
                return 225;
            case ArmState.HPOne:
                return 45;
            default: return 0;
        }
    }
}
