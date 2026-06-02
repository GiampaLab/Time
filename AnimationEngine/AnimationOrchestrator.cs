using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.AnimationEngine.Patterns;
using Time.Components;

namespace Time.AnimationEngine;
public class AnimationOrchestrator(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks)
{
    private readonly IJSRuntime JSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> Clocks = clocks;
    private readonly Random random = new();
    private List<IClockPattern> remainingPatterns = PatternRegistry.All.ToList();
    private IClockPattern? currentPattern;

    public Func<IAnimationManager?, (IAnimationManager animationManager, int duration)> GetNextAnimationManager()
    {
        return animationManager =>
        {
            if (animationManager == null)
            {
                return (GetTimeAnimationManager(), 25000);
            }
            else if (animationManager.GetAnimationType() == AnimationType.Time)
            {
                return (GetPatternAnimationManager(), 13000);
            }
            else if (animationManager.GetAnimationType() == AnimationType.Pattern)
            {
                return (GetInfiniteAnimationManager(), 25000);
            }
            else if (animationManager.GetAnimationType() == AnimationType.Infinite)
            {
                return (GetTimeAnimationManager(), 25000);
            }
            else
            {
                throw new NotImplementedException("Unknown animation type: " + animationManager.GetAnimationType());
            }
        };
    }

    // Plays the continuous motion for the pattern that the Pattern phase just posed.
    private IAnimationManager GetInfiniteAnimationManager() =>
        (currentPattern ?? throw new InvalidOperationException("No pattern selected; the Pattern phase must run before the Infinite phase."))
            .BuildInfinite(JSRuntime, Clocks);

    private IAnimationManager GetPatternAnimationManager()
    {
        return new PatternAnimationManager(JSRuntime, Clocks,
            () =>
            {
                static Components.AnimationConfig SetHourArmAnimationConfig(int index, Direction direction) =>
                    new()
                    {
                        Direction = direction,
                        EasingFunction = "ease-in-out",
                        Duration = 5000,
                        Delay = AnimationConfigs.StaggeredAnimation(index, 0, 300)
                    };

                Array values = Enum.GetValues(typeof(Direction));
                SetRandomClockConfigs(Clocks, index => SetHourArmAnimationConfig(index, (Direction)(values.GetValue(random.Next(values.Length)) ?? Direction.Clockwise)),
                    index => SetHourArmAnimationConfig(index, (Direction)(values.GetValue(random.Next(values.Length)) ?? Direction.Anticlockwise)));

                currentPattern = SelectNextPattern();
                currentPattern.Pose(Clocks);
            });
    }

    private TimeAnimationManager GetTimeAnimationManager()
    {
        return new TimeAnimationManager(JSRuntime, Clocks, () =>
        {
            static Components.AnimationConfig CreateArmAnimationConfig(int index, Direction direction) =>
             new()
             {
                 Direction = direction,
                 EasingFunction = "ease-out",
                 Duration = 8000,
                 Delay = AnimationConfigs.StaggeredAnimation(index, 0, 400)
             };

            Array values = Enum.GetValues(typeof(Direction));
            SetRandomClockConfigs(Clocks,
            index => CreateArmAnimationConfig(index, (Direction)(values.GetValue(random.Next(values.Length)) ?? Direction.Clockwise)),
            index => CreateArmAnimationConfig(index, (Direction)(values.GetValue(random.Next(values.Length)) ?? Direction.Anticlockwise)));
        },
        () =>
        {
            static Components.AnimationConfig CreateStaticConfig(Direction direction) =>
             new()
             {
                 Direction = direction,
                 EasingFunction = "ease-in-out",
                 Duration = 7000,
                 Delay = 0
             };

            AnimationConfigs.SetStaticConfig(Clocks, CreateStaticConfig(Direction.Anticlockwise), CreateStaticConfig(Direction.Clockwise));
        });
    }

    private void SetRandomClockConfigs(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        Array values = Enum.GetValues(typeof(AnimationConfigType));
        object value = values.GetValue(random.Next(values.Length)) ?? AnimationConfigType.Default;
        AnimationConfigType randomAnimationType = (AnimationConfigType)value;

        switch (randomAnimationType)
        {
            case AnimationConfigType.Default:
                AnimationConfigs.SetDefaultConfig(clocks, selectFirstArmConfig, selectSecondArmConfig);
                break;
            case AnimationConfigType.Reverse:
                AnimationConfigs.SetReverseConfig(clocks, selectFirstArmConfig, selectSecondArmConfig);
                break;
            case AnimationConfigType.ByRow:
                AnimationConfigs.SetByRowConfig(clocks, selectFirstArmConfig, selectSecondArmConfig);
                break;
            case AnimationConfigType.Spiral:
                AnimationConfigs.SetSpiralConfig(clocks, selectFirstArmConfig, selectSecondArmConfig);
                break;
            case AnimationConfigType.Snake:
                AnimationConfigs.SetSnakeConfig(clocks, selectFirstArmConfig, selectSecondArmConfig);
                break;
            case AnimationConfigType.CenterOut:
                static Components.AnimationConfig CreateArmAnimationConfig(Clock clock, int index, Direction direction) =>
                        new()
                        {
                            Direction = direction,
                            EasingFunction = "ease-out",
                            Duration = 7000,
                            Delay = AnimationConfigs.StaggeredAnimation(index, 0, 500)
                        };

                static Components.AnimationConfig SetHourArmAnimationConfig(Clock clock, int index) =>
                    CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Clockwise : Direction.Anticlockwise);

                static Components.AnimationConfig SetMinuteArmAnimationConfig(Clock clock, int index) =>
                    CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Anticlockwise : Direction.Clockwise);
                AnimationConfigs.SetCenterOutConfig(clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(randomAnimationType), randomAnimationType, null);
        }
    }

    // Randomly pick a pattern, not repeating any until every registered pattern has been shown.
    private IClockPattern SelectNextPattern()
    {
        if (remainingPatterns.Count == 0)
        {
            remainingPatterns = PatternRegistry.All.ToList();
        }

        IClockPattern pattern = remainingPatterns[random.Next(remainingPatterns.Count)];
        remainingPatterns.Remove(pattern);
        return pattern;
    }
}
