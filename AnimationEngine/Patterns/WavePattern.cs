using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

/// <summary>
/// A field of upright needles that sway from side to side like wind moving over wheat.
/// Every needle shares one swing period; only its phase differs, offset by its position on
/// the grid's diagonal (column + row). That phase gradient turns 24 identical swings into a
/// single crest that travels smoothly across the wall and — because the period is shared —
/// keeps its shape forever instead of drifting apart.
/// The motion uses the shared "animateClockArmSwing" JS primitive (also used by Pendulum)
/// with a 45° amplitude; each needle eases out of its posed upright angle, so the crest
/// builds smoothly across the grid like wind arriving — no snap.
/// </summary>
public sealed class WavePattern : IClockPattern
{
    public string Name => "Wave";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetWavePattern(clocks);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            Components.AnimationConfig Config(int index)
            {
                // index = clockId - 1, laid out column-major: column = index / 3, row = index % 3.
                var column = index / 3; // 0..7
                var row = index % 3;    // 0..2
                var diagonal = column + row; // 0..9 — distance along the wave's travel direction.

                // Shared period (Duration) keeps every needle locked in step; Delay is purely a
                // phase offset. A slightly smaller per-step offset keeps neighbouring needles
                // a touch more in sync, so the crest is a little gentler/flatter as it travels
                // (10 steps × 390ms ≈ 0.8 of the 4400ms full cycle across the diagonal).
                return new()
                {
                    Direction = Direction.Clockwise,
                    EasingFunction = "ease-in-out",
                    Duration = 2200,
                    Delay = diagonal * 390,
                    Amplitude = 45
                };
            }

            // Both arms share the same config so they stay overlapped as one clean needle.
            AnimationConfigs.SetDefaultConfig(clocks, Config, Config);
        }, jsFunctionName: "animateClockArmSwing");
}
