using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

public sealed class TriangularPattern : IClockPattern
{
    public string Name => "Triangular";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetTriangularPattern(clocks);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            static Components.AnimationConfig CreateArmAnimationConfig(Clock clock, int index, Direction direction) =>
                new()
                {
                    Direction = direction,
                    EasingFunction = "ease-in",
                    Duration = 7000,
                    Delay = 0
                };

            static Components.AnimationConfig SetHourArmAnimationConfig(Clock clock, int index) =>
                CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Anticlockwise : Direction.Clockwise);

            static Components.AnimationConfig SetMinuteArmAnimationConfig(Clock clock, int index) =>
                CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Anticlockwise : Direction.Clockwise);

            AnimationConfigs.SetCenterOutConfig(clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);
        });
}
