using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

public sealed class DiagonalPattern : IClockPattern
{
    public string Name => "Diagonal";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetDiagonalPattern(clocks);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            Components.AnimationConfig SetHourArmAnimationConfig(int index) => new()
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "ease-in",
                Duration = 5000,
                Delay = AnimationConfigs.StaggeredAnimation(index, 0, 80)
            };

            AnimationConfigs.SetDefaultConfig(clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
        });
}
