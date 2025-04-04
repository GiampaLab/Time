using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class TimeAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, int delay, int duration,
    bool staggeredDelay, bool staggeredDuration) : IAnimationManager
{
    private DotNetObjectReference<IAnimationManager>? myDotNetObjectReference;
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<Components.AnimationConfig> animationConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();
    private int currentHourFirstDigit = 0;
    private int currentHourSecondDigit = 0;
    private int currentMinuteFirstDigit = 0;
    private int currentMinuteSecondDigit = 0;
    private Timer? timer;
    readonly Func<Clock, int, Components.AnimationConfig> SetHourArmAnimationConfig = (clock, index) =>
        new Components.AnimationConfig
        {
            Direction = Direction.Anticlockwise,
            EasingFunction = "linear",
            Duration = AnimationConfigs.StaggeredAnimation(staggeredDuration, index, duration, 80),
            Delay = AnimationConfigs.StaggeredAnimation(staggeredDelay, index, delay, 80)
        };

    readonly Func<Clock, int, Components.AnimationConfig> SetMinuteArmAnimationConfig = (clock, index) =>
        new Components.AnimationConfig
        {
            Direction = Direction.Clockwise,
            EasingFunction = "linear",
            Duration = AnimationConfigs.StaggeredAnimation(staggeredDuration, index, duration, 80),
            Delay = AnimationConfigs.StaggeredAnimation(staggeredDelay, index, delay, 80)
        };
    public void Start()
    {
        AnimationConfigs.SetClocksAnimationConfigs(clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);
        SetAnimationStatus(null);
    }


    [JSInvokable]
    public void AnimationFinished()
    {
        timer?.Dispose();
        timer = new Timer(SetAnimationStatus, new AutoResetEvent(false), 0, 200);
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
            timer?.Dispose();
            currentHourFirstDigit = hoursFirstDigit;
            currentHourSecondDigit = hoursSecondDigit;
            currentMinuteFirstDigit = minuteFirstDigit;
            currentMinuteSecondDigit = minuteSecondDigit;

            AnimationConfigs.SetNextNumbersAnimationStatus(clocks, currentHourFirstDigit, currentHourSecondDigit, currentMinuteFirstDigit, currentMinuteSecondDigit);

            await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
                animationConfigs.Select(AnimationUtils.MapAnimationConfig));
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

    public void Dispose()
    {
        timer?.Dispose();
    }
}