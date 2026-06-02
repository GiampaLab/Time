using Microsoft.JSInterop;
using Time.Components;

namespace Time.AnimationEngine.Patterns;

/// <summary>
/// A clock-grid pattern: a posed shape plus the endless motion it animates into.
///
/// HOW TO ADD A NEW PATTERN (3 steps, all additive — nothing else needs editing):
///   1. (Optional) Add a "SetXxxPattern" pose helper in
///      <see cref="Time.AnimationConfig.AnimationPatterns"/>, or reuse an existing one.
///   2. Create a new file "XxxPattern.cs" in this folder implementing this interface.
///   3. Add "new XxxPattern()," to <see cref="PatternRegistry.All"/>.
///
/// The orchestrator picks a pattern at random (no repeats until all are shown), calls
/// <see cref="Pose"/> to set the target shape, then calls <see cref="BuildInfinite"/>
/// to play the continuous motion before the clock returns to showing the time.
///
/// CLOCK GRID: ids 1..24 in a 3-row x 8-column, column-major layout
/// (column c in 0..7, row r in 0..2  =>  id = c * 3 + r + 1).
/// See the "Clocks IDs configuration" table in README.MD.
/// </summary>
public interface IClockPattern
{
    /// <summary>Stable, human/agent-readable name. Used for selection logging and docs.</summary>
    string Name { get; }

    /// <summary>
    /// Pose the 24 clocks into the static target shape by setting each arm's angle
    /// (typically via the "SetXxxPattern" helpers on
    /// <see cref="Time.AnimationConfig.AnimationPatterns"/> and Clock.UpdateState).
    /// </summary>
    void Pose(Dictionary<int, Clock> clocks);

    /// <summary>
    /// Build the endless motion that plays after the pose. Reuse the existing distribution
    /// helpers on <see cref="Time.AnimationConfig.AnimationConfigs"/> (SetDefaultConfig,
    /// SetSpiralConfig, SetCenterOutConfig, SetSnakeConfig, StaggeredAnimation, ...) inside
    /// an <see cref="InfiniteAnimationManager"/>. Only pass a non-default
    /// <c>jsFunctionName</c> when the pattern needs a bespoke JS motion primitive
    /// (e.g. the pendulum swing); otherwise the default continuous-spin primitive is used.
    /// </summary>
    IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks);
}
