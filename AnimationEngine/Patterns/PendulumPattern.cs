using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

/// <summary>
/// A wall of clock needles that hang downward then swing back and forth like synchronized
/// metronomes. Clocks within a column share a period/phase while successive columns swing
/// slightly slower and later, so the field drifts in and out of sync (a pendulum-wave).
/// The swing motion uses the bespoke "animateClockArmPendulum" JS primitive.
/// </summary>
public sealed class PendulumPattern : IClockPattern
{
    public string Name => "Pendulum";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetPendulumPattern(clocks);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            Components.AnimationConfig Config(int index)
            {
                // Clocks share a period/phase within a column (3 clocks each),
                // while successive columns swing slightly slower and later so the
                // wall of pendulums drifts in and out of sync (pendulum-wave).
                var col = index / 3;
                return new()
                {
                    Direction = Direction.Clockwise,
                    EasingFunction = "ease-in-out",
                    Duration = 2600 + col * 120,
                    Delay = col * 180
                };
            }

            AnimationConfigs.SetDefaultConfig(clocks, Config, Config);
        }, jsFunctionName: "animateClockArmPendulum");
}
