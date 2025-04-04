using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;
public class InfiniteAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks,
    Direction firstArmDirection, Direction secondArmDirection, int delay, int duration,
    bool staggeredDelay, bool staggeredDuration) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<Components.AnimationConfig> animationConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();

    readonly Func<Clock, int, Components.AnimationConfig> SetHourArmAnimationConfig = (clock, index) =>
        new Components.AnimationConfig
        {
            Direction = firstArmDirection,
            EasingFunction = "linear",
            Duration = AnimationConfigs.StaggeredAnimation(staggeredDuration, index, duration, 80),
            Delay = AnimationConfigs.StaggeredAnimation(staggeredDelay, index, delay, 80)
        };

    readonly Func<Clock, int, Components.AnimationConfig> SetMinuteArmAnimationConfig = (clock, index) =>
        new Components.AnimationConfig
        {
            Direction = secondArmDirection,
            EasingFunction = "linear",
            Duration = AnimationConfigs.StaggeredAnimation(staggeredDuration, index, duration, 80),
            Delay = AnimationConfigs.StaggeredAnimation(staggeredDelay, index, delay, 80)
        };
    public async void Start()
    {
        AnimationConfigs.SetClocksAnimationConfigs(clocks,
            SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);

        var args = animationConfigs.Select(AnimationUtils.MapAnimationConfig);

        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArmInfinite", null, args);
    }

    [JSInvokable]
    public void AnimationFinished()
    {
    }

    public AnimationType GetAnimationType()
    {
        return AnimationType.Infinite;
    }

    public void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference)
    {
    }

    public void Stop()
    {
    }

    public void Dispose()
    {
    }
}