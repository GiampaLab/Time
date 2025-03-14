using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;
using Time.Utils;

namespace Time.AnimationEngine;

public class AnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, List<ElementReference> hourReferences, List<ElementReference> minuteReferences)
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;

    private readonly IList<ArmConfig> armConfigs = clocks.Values.Select(x =>
            x.FirstArm.Config).ToList().Union(clocks.Values.Select(x => x.SecondArm.Config).ToList()).ToList();

    private int currentHourFirstDigit = 0;
    private int currentHourSecondDigit = 0;
    private int currentMinuteFirstDigit = 0;
    private int currentMinuteSecondDigit = 0;

    public void Start()
    {
        AnimationConfigs.SetClocksConfigs(clocks,
            new ArmConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "ease-in-out",
                Duration = 3000
            },
            new ArmConfig
            {
                Direction = Direction.Clockwise,
                EasingFunction = "ease-in-out",
                Duration = 3000
            }, 60, hourReferences, minuteReferences);

        var timer = new Timer(SetAnimationStatus, new AutoResetEvent(false), 0, 500);
    }

    private async void SetAnimationStatus(object? stateInfo)
    {
        var time = DateTime.Now;
        var hoursFirstDigit = time.Hour / 10;
        var hoursSecondDigit = time.Hour % 10;
        var minuteFirstDigit = time.Second / 10;
        var minuteSecondDigit = time.Minute % 10;

        if (hoursFirstDigit != currentHourFirstDigit || hoursSecondDigit != currentHourSecondDigit || minuteFirstDigit != currentMinuteFirstDigit || minuteSecondDigit != currentMinuteSecondDigit)
        {
            currentHourFirstDigit = hoursFirstDigit;
            currentHourSecondDigit = hoursSecondDigit;
            currentMinuteFirstDigit = minuteFirstDigit;
            currentMinuteSecondDigit = minuteSecondDigit;

            AnimationConfigs.SetNextNumbersAnimationStatus(clocks, currentHourFirstDigit, currentHourSecondDigit, currentMinuteFirstDigit, currentMinuteSecondDigit);
            await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm",
                (object)armConfigs.Select(config => new
                {
                    state = config.State,
                    elementReference = config.ElementReference,
                    easing = config.EasingFunction,
                    direction = Enum.GetName(typeof(Direction), config.Direction),
                    duration = config.Duration
                }
                    ).ToArray());
        }
    }
}