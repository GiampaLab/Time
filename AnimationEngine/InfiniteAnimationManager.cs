using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.AnimationEngine;
using Time.Components;

public class InfiniteAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks,
    List<ElementReference> hourReferences, List<ElementReference> minuteReferences,
    Direction firstArmDirection, Direction secondArmDirection, bool staggeredDelay) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<AnimationConfig> animationConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();

    public async void Start()
    {
        AnimationConfigs.SetClocksConfigs(clocks,
            new AnimationConfig
            {
                Direction = firstArmDirection,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 0
            },
            new AnimationConfig
            {
                Direction = secondArmDirection,
                EasingFunction = "linear",
                Duration = 5000,
                Delay = 0
            }, 60, hourReferences, minuteReferences);

        var args = animationConfigs.Select((config, index) => new
        {
            state = config.State,
            elementReference = config.ElementReference,
            easing = config.EasingFunction,
            direction = Enum.GetName(typeof(Direction), config.Direction),
            duration = config.Duration,
            delay = staggeredDelay ? config.Delay + (index % 2) == 1 ? (index - 1) * 50 : index * 50 : config.Delay
        }).ToArray();

        Console.WriteLine("armConfigs: " + args.Length);
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