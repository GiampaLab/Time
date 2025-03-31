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
    private readonly IList<Components.AnimationConfig> animationConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();
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

        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
            animationConfigs.Select((config, index) => new
            {
                state = config.State,
                elementReference = config.ElementReference,
                easing = config.EasingFunction,
                direction = Enum.GetName(typeof(Direction), config.Direction),
                duration = config.Duration,
                delay = staggeredDelay ? config.Delay + (index % 2) == 1 ? (index - 1) * 50 : index * 50 : config.Delay
            }
                ).ToArray());
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