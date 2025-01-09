namespace Time.AnimationEngine;
using Time.Utils;

public class AnimationLifecycleManager
{
    private bool? animationSetupCompleted = false;
    private bool animationSettingsCompleted = false;
    private List<AnimationLifecycleManager> animationLifecycleManagers = [];
    private DelayAnimation chainedAnimationInterval { get; set; } = new DelayAnimation();
    private DelayAnimation chainedAnimationLength { get; set; } = new DelayAnimation();
    private Random rnd = new Random();
    private int chainedAnimationsIndex = 0;
    public delegate bool? CallbackEventHandler(AnimationStatus status, double timeElapsed);
    public event CallbackEventHandler? OnStatusChanged;
    public event EventHandler? OnSettingsCompleted;
    public DelayAnimation DelayAnimation { get; set; } = new DelayAnimation();

    public void OnNextAnimationSettingsCompleted(object? sender, EventArgs args)
    {
        animationSettingsCompleted = false;
        animationSetupCompleted = false;
        DelayAnimation.Started = false;
    }

    public void Start(string animationName, double timeElapsed)
    {
        //checks if and when to run chained animations loop
        if (animationLifecycleManagers.Count() > 0 && chainedAnimationInterval.TimeIsUp(timeElapsed))
        {
            if (!chainedAnimationLength.TimeIsUp(timeElapsed))
                animationLifecycleManagers[chainedAnimationsIndex].Start($"anim {chainedAnimationsIndex}", timeElapsed);
            else
            {
                //Reset the chainedAnimationInterval
                chainedAnimationInterval.Started = false;
                chainedAnimationsIndex = chainedAnimationsIndex < animationLifecycleManagers.Count() - 1 ? ++chainedAnimationsIndex : 0;
            }
        }
        //animation loop
        else
        {
            //Reset the chainedAnimationLength
            chainedAnimationLength.Started = false;
            if (animationSetupCompleted.HasValue && animationSetupCompleted.Value)
            {
                if (!animationSettingsCompleted)
                {
                    animationSettingsCompleted = true;
                    OnSettingsCompleted?.Invoke(this, EventArgs.Empty);
                    OnStatusChanged?.Invoke(AnimationStatus.PerformPreAnimationSettings, timeElapsed);
                }
                if (DelayAnimation.TimeIsUp(timeElapsed))
                    OnStatusChanged?.Invoke(AnimationStatus.StartAnimation, timeElapsed);
            }
            else
                animationSetupCompleted = OnStatusChanged?.Invoke(AnimationStatus.StartInitAnimation, timeElapsed);
        }
    }

    public void Chain(List<AnimationLifecycleManager> animationLifecycleManagers, int intervalMillisec, int lengthMillisec)
    {
        this.animationLifecycleManagers = animationLifecycleManagers;
        chainedAnimationLength.DelayMillisec = lengthMillisec;
        chainedAnimationInterval.DelayMillisec = intervalMillisec;
        chainedAnimationsIndex = rnd.Next(animationLifecycleManagers.Count() - 1);
    }
}

public enum AnimationStatus
{
    StartInitAnimation,
    StartAnimation,
    PerformPreAnimationSettings
}