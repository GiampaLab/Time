using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.AnimationEngine;
using Time.Components;

public class TimePatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks,
    List<ElementReference> hourReferences, List<ElementReference> minuteReferences,
    Direction firstArmDirection, Direction secondArmDirection) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<AnimationConfig> armConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();
    private DotNetObjectReference<IAnimationManager>? myDotNetObjectReference;

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

        var time = DateTime.Now;
        var hoursFirstDigit = time.Hour / 10;
        var hoursSecondDigit = time.Hour % 10;
        var minuteFirstDigit = time.Second / 10;
        var minuteSecondDigit = time.Minute % 10;

        AnimationConfigs.SetNextNumbersAnimationStatus(clocks, hoursFirstDigit, hoursSecondDigit, minuteFirstDigit, minuteSecondDigit);

        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
            armConfigs.Select((config, index) => new
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

    public void AnimationFinished()
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

    public void Stop()
    {
    }
}