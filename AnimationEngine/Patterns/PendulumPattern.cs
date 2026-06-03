using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

/// <summary>
/// A wall of clock needles that hang downward then swing back and forth like synchronized
/// metronomes. Every column shares the same swing period, so the only difference between
/// columns is a constant start delay — this yields a coherent travelling wave that keeps
/// its phase relationship indefinitely (it never drifts out of sync).
/// The swing motion uses the shared "animateClockArmSwing" JS primitive with a 55° amplitude.
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
                // The 3 clocks in a column share one period and phase. Every column uses the
                // SAME period (no per-column duration drift) and differs only by a constant
                // start delay, so the staggered launch becomes a travelling wave that stays
                // connected forever instead of drifting apart over time.
                var col = index / 3;
                return new()
                {
                    Direction = Direction.Clockwise,
                    EasingFunction = "ease-in-out",
                    Duration = 2600,
                    Delay = col * 260,
                    Amplitude = 55
                };
            }

            AnimationConfigs.SetDefaultConfig(clocks, Config, Config);
        }, jsFunctionName: "animateClockArmSwing");
}
