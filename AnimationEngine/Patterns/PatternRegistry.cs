namespace Time.AnimationEngine.Patterns;

/// <summary>
/// The single source of truth for which clock patterns exist.
///
/// TO REGISTER A NEW PATTERN: add one line below with its class.
/// That is the ONLY edit needed besides creating the pattern's own file
/// (see <see cref="IClockPattern"/> for the full 3-step recipe).
/// Order is irrelevant — the orchestrator selects patterns at random.
/// </summary>
public static class PatternRegistry
{
    public static IReadOnlyList<IClockPattern> All { get; } = new List<IClockPattern>
    {
        new SquaresPattern(),
        new LinePattern(),
        new FlowPattern(),
        new FlowerPattern(),
        new DiagonalPattern(),
        new NumbersPattern(),
        new TriangularPattern(),
        new PendulumPattern(),
        new WavePattern(),
    };
}
