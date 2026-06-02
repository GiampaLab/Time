using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

public sealed class LinePattern : IClockPattern
{
    public string Name => "Line";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetLinePattern(clocks);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            Func<int, Components.AnimationConfig> CreateConfig(Direction direction) => index => new()
            {
                Direction = direction,
                EasingFunction = "ease-in",
                Duration = 5000,
                Delay = AnimationConfigs.StaggeredAnimation(index, 0, 40)
            };

            AnimationConfigs.SetDefaultConfig(clocks, CreateConfig(Direction.Anticlockwise), CreateConfig(Direction.Clockwise));
        });
}
