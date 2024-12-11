namespace Time.Utils;
public class EasingAnimation
{
    public Func<float, float> EasingFunction { get; set; }
    private int framesCount = 0;
    private readonly int totalFrames = 300;
    public EasingAnimation(Func<float, float> easingFunction)
    {
        EasingFunction = easingFunction;
    }

    public int GetEasingValue()
    {
        framesCount++;
        float progress = (float)framesCount / totalFrames;
        float previousProgress = (float)(framesCount - 1) / totalFrames;
        if (progress <= 1)
        {
            var deltaProgress = (EasingFunction(progress) * totalFrames) - (EasingFunction(previousProgress) * totalFrames);
            var returnValue = (int)Math.Round(deltaProgress, 0);
            return returnValue == 0 ? 1 : returnValue;
        }
        else
            return 1;
    }

    public void ResetEasingAnimation()
    {
        framesCount = 0;
    }
}