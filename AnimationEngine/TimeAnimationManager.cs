using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class TimeAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, Action SetTimeAnimationConfig, Action SetStaticTimeAnimationConfig) : IAnimationManager
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
    public bool IsFinished { get; private set; } = false;
    public void Start()
    {
        SetTimeAnimationConfig();
        SetAnimationStatus(null);
        SetStaticTimeAnimationConfig();
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
        var minuteSecondDigit = time.Minute % 10;

        if (hoursFirstDigit != currentHourFirstDigit || hoursSecondDigit != currentHourSecondDigit || minuteFirstDigit != currentMinuteFirstDigit || minuteSecondDigit != currentMinuteSecondDigit)
        {
            timer?.Dispose();
            currentHourFirstDigit = hoursFirstDigit;
            currentHourSecondDigit = hoursSecondDigit;
            currentMinuteFirstDigit = minuteFirstDigit;
            currentMinuteSecondDigit = minuteSecondDigit;

            AnimationPatterns.SetNumbersPattern(clocks, currentHourFirstDigit, currentHourSecondDigit, currentMinuteFirstDigit, currentMinuteSecondDigit);

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