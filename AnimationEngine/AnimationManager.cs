using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class AnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, List<ElementReference> hourReferences, List<ElementReference> minuteReferences) : IAnimationManager
{
    private DotNetObjectReference<IAnimationManager>? dotNetObjectReference;
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;

    private readonly IList<ArmConfig> armConfigs = clocks.Values.Select(x =>
            x.FirstArm.Config).ToList().Union(clocks.Values.Select(x => x.SecondArm.Config).ToList()).ToList();

    private int currentHourFirstDigit = 0;
    private int currentHourSecondDigit = 0;
    private int currentMinuteFirstDigit = 0;
    private int currentMinuteSecondDigit = 0;
    private Timer timer;
    private bool animationFinished = true;

    public void Start()
    {
        dotNetObjectReference ??= DotNetObjectReference.Create<IAnimationManager>(this);
        AnimationConfigs.SetClocksConfigs(clocks,
            new ArmConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "ease-out",
                Duration = 3000
            },
            new ArmConfig
            {
                Direction = Direction.Clockwise,
                EasingFunction = "ease-out",
                Duration = 3000
            }, 60, hourReferences, minuteReferences);

        timer = new Timer(SetAnimationStatus, new AutoResetEvent(false), 0, 500);
    }


    [JSInvokable]
    public void AnimationFinished()
    {
        animationFinished = true;
    }

    public void Stop()
    {
        timer.Dispose();
    }

    public void Continue()
    {

    }

    private async void SetAnimationStatus(object? stateInfo)
    {
        var time = DateTime.Now;
        var hoursFirstDigit = time.Hour / 10;
        var hoursSecondDigit = time.Hour % 10;
        var minuteFirstDigit = time.Second / 10;
        var minuteSecondDigit = time.Minute % 10;

        if (animationFinished && (hoursFirstDigit != currentHourFirstDigit || hoursSecondDigit != currentHourSecondDigit || minuteFirstDigit != currentMinuteFirstDigit || minuteSecondDigit != currentMinuteSecondDigit))
        {
            animationFinished = false;

            currentHourFirstDigit = hoursFirstDigit;
            currentHourSecondDigit = hoursSecondDigit;
            currentMinuteFirstDigit = minuteFirstDigit;
            currentMinuteSecondDigit = minuteSecondDigit;

            AnimationConfigs.SetNextNumbersAnimationStatus(clocks, currentHourFirstDigit, currentHourSecondDigit, currentMinuteFirstDigit, currentMinuteSecondDigit);

            await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", dotNetObjectReference,
                armConfigs.Select(config => new
                {
                    state = config.State,
                    elementReference = config.ElementReference,
                    easing = config.EasingFunction,
                    direction = Enum.GetName(typeof(Direction), config.Direction),
                    duration = config.Duration
                }
                    ).ToArray(), false);
        }
    }
}