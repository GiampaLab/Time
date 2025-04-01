using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class PatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks,
    List<ElementReference> hourReferences, List<ElementReference> minuteReferences,
    Action<Dictionary<int, Clock>> SetPatternAnimationStatus, Direction hourArmDirection, Direction minuteArmDirection, bool staggeredDelay) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<Components.AnimationConfig> animationInfo = clocks.Reverse().ToDictionary().Values.SelectMany(x =>
            new[] { x.SecondArm.Config, x.FirstArm.Config }).ToArray();
    private DotNetObjectReference<IAnimationManager>? myDotNetObjectReference;

    public async void Start()
    {
        AnimationConfigs.SetClocksConfigs(clocks,
            new Components.AnimationConfig
            {
                Direction = hourArmDirection,
                EasingFunction = "linear",
                Duration = 4500,
                Delay = 0
            },
            new Components.AnimationConfig
            {
                Direction = minuteArmDirection,
                EasingFunction = "linear",
                Duration = 4500,
                Delay = 0
            }, 60, hourReferences, minuteReferences);

        SetPatternAnimationStatus(clocks);

        var animationConfigsArray = animationInfo.Select((config, index) => new
        {
            state = config.State,
            elementReference = config.ElementReference,
            easing = config.EasingFunction,
            direction = Enum.GetName(typeof(Direction), config.Direction),
            duration = config.Duration,
            delay = staggeredDelay ? config.Delay + (index % 2) == 1 ? (index - 1) * 80 : index * 80 : config.Delay
        });

        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
            animationConfigsArray.Reverse().ToArray());
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
}