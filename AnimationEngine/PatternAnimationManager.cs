using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class PatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, List<ElementReference> hourReferences, List<ElementReference> minuteReferences) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<Components.AnimationConfig> armConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();
    private DotNetObjectReference<IAnimationManager>? myDotNetObjectReference;

    public async void Start()
    {
        AnimationConfigs.SetClocksConfigs(clocks,
            new Components.AnimationConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 0
            },
            new Components.AnimationConfig
            {
                Direction = Direction.Clockwise,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 0
            }, 60, hourReferences, minuteReferences);

        AnimationConfigs.SetNextPatternAnimationStatus(clocks);
        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
            armConfigs.Select(config => new
            {
                state = config.State,
                elementReference = config.ElementReference,
                easing = config.EasingFunction,
                direction = Enum.GetName(typeof(Direction), config.Direction),
                duration = config.Duration,
                delay = config.Delay
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