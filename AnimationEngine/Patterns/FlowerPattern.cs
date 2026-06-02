using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

public sealed class FlowerPattern : IClockPattern
{
    public string Name => "Flower";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetFlowerPattern(clocks);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            static Components.AnimationConfig CreateArmAnimationConfig(Clock clock, int index, Direction direction) =>
                new()
                {
                    Direction = direction,
                    EasingFunction = "ease-in",
                    Duration = 7000,
                    Delay = AnimationConfigs.StaggeredAnimation(index, 0, 400)
                };

            static Components.AnimationConfig SetHourArmAnimationConfig(Clock clock, int index) =>
                CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Clockwise : Direction.Anticlockwise);

            static Components.AnimationConfig SetMinuteArmAnimationConfig(Clock clock, int index) =>
                CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Anticlockwise : Direction.Clockwise);

            AnimationConfigs.SetCenterOutConfig(clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);
        });
}
