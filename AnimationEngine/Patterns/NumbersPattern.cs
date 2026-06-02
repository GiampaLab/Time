using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

public sealed class NumbersPattern : IClockPattern
{
    public string Name => "Numbers";

    public void Pose(Dictionary<int, Clock> clocks)
    {
        var time = DateTime.Now;
        var hoursFirstDigit = time.Hour / 10;
        var hoursSecondDigit = time.Hour % 10;
        var minuteFirstDigit = time.Minute / 10;
        var minuteSecondDigit = time.Minute % 10;
        AnimationPatterns.SetNumbersPattern(clocks, hoursFirstDigit, hoursSecondDigit, minuteFirstDigit, minuteSecondDigit, 90);
    }

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            Components.AnimationConfig SetHourArmAnimationConfig(int index) => new Components.AnimationConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "ease-in",
                Duration = 7000,
                Delay = 0
            };

            AnimationConfigs.SetDefaultConfig(clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
        });
}
