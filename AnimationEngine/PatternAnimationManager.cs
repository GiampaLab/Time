using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class PatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, List<ElementReference> hourReferences, List<ElementReference> minuteReferences) : IAnimationManager
{
    private DotNetObjectReference<IAnimationManager> dotNetObjectReference;
    private bool animationFinished = true;
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;

    private readonly IList<ArmConfig> armConfigs = clocks.Values.Select(x =>
            x.FirstArm.Config).ToList().Union(clocks.Values.Select(x => x.SecondArm.Config).ToList()).ToList();

    public async void Start()
    {
        dotNetObjectReference ??= DotNetObjectReference.Create<IAnimationManager>(this);
        AnimationConfigs.SetClocksConfigs(clocks,
            new ArmConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "ease-in-out",
                Duration = 5000,
                Delay = 1000
            },
            new ArmConfig
            {
                Direction = Direction.Clockwise,
                EasingFunction = "ease-in-out",
                Duration = 5000,
                Delay = 1000
            }, 60, hourReferences, minuteReferences);

        AnimationConfigs.SetNextPatternAnimationStatus(clocks);
        if (animationFinished)
        {
            animationFinished = false;
            await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", dotNetObjectReference,
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
        else
        {
            await Task.Delay(1000);
            Start();
        }
    }

    [JSInvokable]
    public void AnimationFinished()
    {
        animationFinished = true;
    }

    public async void Stop()
    {
        await jSRuntime.InvokeVoidAsync("animationLoop.pauseClockArmAnimation");
    }
}