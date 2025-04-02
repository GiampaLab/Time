using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class TimeAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks,
    List<ElementReference> hourReferences, List<ElementReference> minuteReferences,
    bool staggeredDelay, bool staggeredDuration) : IAnimationManager
{
    private DotNetObjectReference<IAnimationManager>? myDotNetObjectReference;
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly bool staggeredDelay = staggeredDelay;
    private readonly bool staggeredDuration = staggeredDuration;
    private readonly IList<Components.AnimationConfig> animationConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();
    private int currentHourFirstDigit = 0;
    private int currentHourSecondDigit = 0;
    private int currentMinuteFirstDigit = 0;
    private int currentMinuteSecondDigit = 0;
    private Timer? timer;
    public void Start()
    {
        AnimationConfigs.SetClocksConfigs(clocks,
            new Components.AnimationConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "ease-out",
                Duration = 4500,
                Delay = 0
            },
            new Components.AnimationConfig
            {
                Direction = Direction.Clockwise,
                EasingFunction = "ease-out",
                Duration = 4500,
                Delay = 0
            }, 60, hourReferences, minuteReferences);
        SetAnimationStatus(null);
    }


    [JSInvokable]
    public void AnimationFinished()
    {
        timer?.Dispose();
        timer = new Timer(SetAnimationStatus, new AutoResetEvent(false), 0, 500);
    }

    public void Stop()
    {
        timer?.Dispose();
    }

    private async void SetAnimationStatus(object? stateInfo)
    {
        var time = DateTime.Now;
        var hoursFirstDigit = time.Hour / 10;
        var hoursSecondDigit = time.Hour % 10;
        var minuteFirstDigit = time.Minute / 10;
        var minuteSecondDigit = time.Second / 10;

        if (hoursFirstDigit != currentHourFirstDigit || hoursSecondDigit != currentHourSecondDigit || minuteFirstDigit != currentMinuteFirstDigit || minuteSecondDigit != currentMinuteSecondDigit)
        {
            currentHourFirstDigit = hoursFirstDigit;
            currentHourSecondDigit = hoursSecondDigit;
            currentMinuteFirstDigit = minuteFirstDigit;
            currentMinuteSecondDigit = minuteSecondDigit;

            AnimationConfigs.SetNextNumbersAnimationStatus(clocks, currentHourFirstDigit, currentHourSecondDigit, currentMinuteFirstDigit, currentMinuteSecondDigit);

            await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
                animationConfigs.Select((config, index) => new
                {
                    state = config.State,
                    elementReference = config.ElementReference,
                    easing = config.EasingFunction,
                    direction = Enum.GetName(typeof(Direction), config.Direction),
                    duration = staggeredDuration ? config.Duration + ((index % 2) == 1 ? (index - 1) * 80 : index * 80) : config.Duration,
                    delay = staggeredDelay ? config.Delay + (index % 2) == 1 ? (index - 1) * 80 : index * 80 : config.Delay
                }
                    ).ToArray());
        }
    }

    public AnimationType GetAnimationType()
    {
        return AnimationType.Time;
    }

    public void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference)
    {
        myDotNetObjectReference = dotNetObjectReference;
    }
}