using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.AnimationEngine;
using Time.Components;

public class WavePatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, List<ElementReference> hourReferences, List<ElementReference> minuteReferences) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<AnimationConfig> armConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();

    public async void Start()
    {
        AnimationConfigs.SetClocksConfigs(clocks,
            new AnimationConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 1000
            },
            new AnimationConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 1000
            }, 60, hourReferences, minuteReferences);

        AnimationConfigs.SetNextWaveAnimationStatus(clocks);
        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", null,
            armConfigs.Select((config, index) => new
            {
                state = config.State,
                elementReference = config.ElementReference,
                easing = config.EasingFunction,
                direction = Enum.GetName(typeof(Direction), config.Direction),
                duration = config.Duration,
                delay = config.Delay + (index % 2) == 1 ? (index - 1) * 50 : index * 50
            }
                ).ToArray(), true);
    }

    public void AnimationFinished()
    {
    }

    public AnimationType GetAnimationType()
    {
        return AnimationType.Pattern;
    }

    public void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference)
    {
    }

    public void Stop()
    {
    }
}