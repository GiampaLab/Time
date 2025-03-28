using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class PatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, List<ElementReference> hourReferences, List<ElementReference> minuteReferences) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<ArmConfig> armConfigs = clocks.Values.Select(x =>
            x.FirstArm.Config).ToList().Union(clocks.Values.Select(x => x.SecondArm.Config).ToList()).ToList();

    public async void Start()
    {
        AnimationConfigs.SetClocksConfigs(clocks,
            new ArmConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 1000
            },
            new ArmConfig
            {
                Direction = Direction.Clockwise,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 1000
            }, 60, hourReferences, minuteReferences);

        AnimationConfigs.SetNextPatternAnimationStatus(clocks);
        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", null,
            armConfigs.Select(config => new
            {
                state = config.State,
                elementReference = config.ElementReference,
                easing = config.EasingFunction,
                direction = Enum.GetName(typeof(Direction), config.Direction),
                duration = config.Duration,
                delay = config.Delay
            }
                ).ToArray(), true);
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
    }
}