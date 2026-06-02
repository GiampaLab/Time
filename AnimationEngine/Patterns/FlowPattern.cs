using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

public sealed class FlowPattern : IClockPattern
{
    public string Name => "Flow";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetFlowPattern(clocks);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            Components.AnimationConfig SetHourArmAnimationConfig(int index) => new Components.AnimationConfig
            {
                Direction = Direction.Anticlockwise,
                EasingFunction = "ease-in",
                Duration = 5000,
                Delay = AnimationConfigs.StaggeredAnimation(index, 0, 40)
            };

            AnimationConfigs.SetSpiralConfig(clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
        });
}
