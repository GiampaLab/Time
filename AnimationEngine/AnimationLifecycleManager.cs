namespace Time.AnimationEngine;

using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;
using Time.Utils;

public class AnimationLifecycleManager
{
    private bool? animationSetupCompleted = false;
    private bool animationSettingsCompleted = false;
    private List<AnimationLifecycleManager> animationLifecycleManagers = [];
    private DelayAnimation chainedAnimationInterval = new();
    private DelayAnimation chainedAnimationLength = new();
    private Random rnd = new();
    private int chainedAnimationsIndex = 0;
    private readonly IJSRuntime jSRuntime;
    private readonly IList<AnimationConfig> armConfigs;

    public AnimationLifecycleManager(IJSRuntime jSRuntime, IList<AnimationConfig> armConfigs)
    {
        this.jSRuntime = jSRuntime;
        this.armConfigs = armConfigs;
    }

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

    public async void Start(string animationName, double timeElapsed)
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
                {
                    OnStatusChanged?.Invoke(AnimationStatus.StartAnimation, timeElapsed);
                    await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", (object)armConfigs.Select(config => new { state = config.State, elementReference = config.ElementReference }).ToArray());
                }
            }
            else
            {
                OnStatusChanged?.Invoke(AnimationStatus.StartInitAnimation, timeElapsed);
                animationSetupCompleted = true;
            }
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