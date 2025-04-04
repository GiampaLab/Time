using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class PatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks,
    Action<Dictionary<int, Clock>> SetPatternAnimationStatus, Direction hourArmDirection,
    Direction minuteArmDirection, int duration, int delay,
    bool staggeredDelay, bool staggeredDuration) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<Components.AnimationConfig> animationInfo = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();
    private DotNetObjectReference<IAnimationManager>? myDotNetObjectReference;

    readonly Func<Clock, int, Components.AnimationConfig> SetHourArmAnimationConfig = (clock, index) =>
        new Components.AnimationConfig
        {
            Direction = hourArmDirection,
            EasingFunction = "linear",
            Duration = staggeredDuration ? duration + ((index % 2) == 1 ? (index - 1) * 80 : index * 80) : duration,
            Delay = staggeredDelay ? delay + (index % 2) == 1 ? (index - 1) * 80 : index * 80 : delay
        };

    readonly Func<Clock, int, Components.AnimationConfig> SetMinuteArmAnimationConfig = (clock, index) =>
        new Components.AnimationConfig
        {
            Direction = minuteArmDirection,
            EasingFunction = "linear",
            Duration = staggeredDuration ? duration + ((index % 2) == 1 ? (index - 1) * 80 : index * 80) : duration,
            Delay = staggeredDelay ? delay + (index % 2) == 1 ? (index - 1) * 80 : index * 80 : delay
        };
    public async void Start()
    {
        AnimationConfigs.SetReverseClocksConfigs(clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);

        SetPatternAnimationStatus(clocks);

        var animationConfigsArray = animationInfo.Select((config, index) => new
        {
            state = config.State,
            elementReference = config.ElementReference,
            easing = config.EasingFunction,
            direction = Enum.GetName(typeof(Direction), config.Direction),
            duration = config.Duration,
            delay = config.Delay
        });

        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
            animationConfigsArray.ToArray());
    }

    [JSInvokable]
    public void AnimationFinished()
    {
    }

    public void Stop()
    {
    }

    public AnimationType GetAnimationType()
    {
        return AnimationType.Pattern;
    }

    public void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference)
    {
        myDotNetObjectReference = dotNetObjectReference;
    }

    public void Dispose()
    {
    }
}