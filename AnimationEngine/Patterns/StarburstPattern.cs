using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

/// <summary>
/// A radial sunburst that slowly breathes. The 24 clocks pose as rays pointing outward from
/// the grid centre (this reuses Flow's pose rotated 180° — Flow's pose is the same radial star
/// pointing inward), then each clock's two arms bloom apart into a V and close back to the
/// single ray, over and over. The whole field inhales and exhales together — calm, meditative.
///
/// It deliberately SHARES Flow's pose but uses completely different motion: Flow spins each
/// needle through full rotations (which washes the radial shape out over time), whereas this
/// pattern's motion is BOUNDED — the arms only ever sway between the ray and the open V, so the
/// sunburst stays readable the entire time. Bounded motion is the dividing line.
/// Uses the bespoke "animateClockArmBreathe" JS primitive.
/// </summary>
public sealed class StarburstPattern : IClockPattern
{
    // Half-angle (degrees) each arm sweeps from the closed ray to the open V (V spans 2×this).
    private const int BloomHalfAngle = 65;

    // One full inhale+exhale is 2× this (the alternate animation plays open then closed).
    private const int BreathDuration = 4000;

    // Phase stagger across the grid: 0 = the whole sunburst breathes in unison; raise it (e.g.
    // 250) to make the breath a center-out ripple radiating outward. One-number feel switch.
    private const int RippleDelayPerUnit = 400;

    // Grid centre, in (column, row) space, for the ripple's distance-from-centre stagger.
    private const double CentreColumn = 3.5;
    private const double CentreRow = 1.0;

    public string Name => "Starburst";

    public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetFlowPattern(clocks, 180);

    public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
        new InfiniteAnimationManager(js, clocks, () =>
        {
            static int BreathDelay(int index)
            {
                var dx = index / 3 - CentreColumn; // column 0..7
                var dy = index % 3 - CentreRow;    // row 0..2
                var distance = Math.Sqrt(dx * dx + dy * dy);
                return (int)Math.Round(distance * RippleDelayPerUnit);
            }

            // The two arms bloom in opposite directions (+A / -A) around the shared resting
            // ray, so each clock opens into a symmetric V and closes back to a single needle.
            Components.AnimationConfig Config(int index, int amplitude) => new()
            {
                Direction = Direction.Clockwise,
                EasingFunction = "ease-in-out",
                Duration = BreathDuration,
                Delay = BreathDelay(index),
                Amplitude = amplitude
            };

            AnimationConfigs.SetDefaultConfig(clocks,
                index => Config(index, BloomHalfAngle),
                index => Config(index, -BloomHalfAngle));
        }, jsFunctionName: "animateClockArmBreathe");
}
