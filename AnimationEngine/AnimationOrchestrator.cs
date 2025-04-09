using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.AnimationEngine;
using Time.Components;

namespace Time.AnimationEngine;
public class AnimationOrchestrator(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks)
{
    private readonly IJSRuntime JSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> Clocks = clocks;
    private readonly Random random = new();

    public Func<IAnimationManager?, (IAnimationManager animationManager, int duration)> GetNextAnimationManager()
    {
        return animationManager =>
        {
            if (animationManager == null)
            {
                return (GetTimeAnimationManager(), 30000);
            }
            else if (animationManager.GetAnimationType() == AnimationType.Time)
            {
                return (GetPatternAnimationManager(), 10000);
            }
            else if (animationManager.GetAnimationType() == AnimationType.Pattern)
            {
                return (GetInfiniteAnimationManager((PatternAnimationManager)animationManager), 20000);
            }
            else if (animationManager.GetAnimationType() == AnimationType.Infinite)
            {
                return (GetTimeAnimationManager(), 30000);
            }
            else
            {
                throw new NotImplementedException("Unknown animation type: " + animationManager.GetAnimationType());
            }
        };
    }

    private IAnimationManager GetInfiniteAnimationManager(PatternAnimationManager animationManager)
    {
        return animationManager.AnimationPatternType switch
        {
            AnimationPatternType.Flower => new InfiniteAnimationManager(JSRuntime, Clocks,
                        clocks =>
                        {
                            static Components.AnimationConfig CreateArmAnimationConfig(Clock clock, int index, Direction direction) =>
                                new()
                                {
                                    Direction = direction,
                                    EasingFunction = "linear",
                                    Duration = 7000,
                                    Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 500)
                                };

                            static Components.AnimationConfig SetHourArmAnimationConfig(Clock clock, int index) =>
                                CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Clockwise : Direction.Anticlockwise);

                            static Components.AnimationConfig SetMinuteArmAnimationConfig(Clock clock, int index) =>
                                CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Anticlockwise : Direction.Clockwise);
                            Console.WriteLine("SetFlowerPattern");
                            AnimationConfigs.SetCenterOutConfig(clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);
                        }),

            AnimationPatternType.Line => new InfiniteAnimationManager(JSRuntime, Clocks, (Dictionary<int, Clock>
            clocks) =>
            {
                Components.AnimationConfig SetHourArmAnimationConfig(int index) => new Components.AnimationConfig
                {
                    Direction = Direction.Anticlockwise,
                    EasingFunction = "linear",
                    Duration = 5000,
                    Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 40)
                };
                Console.WriteLine("SetLinePattern");
                AnimationConfigs.SetDefaultConfig(clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
            }),

            AnimationPatternType.Squares => new InfiniteAnimationManager(JSRuntime, Clocks, clocks =>
                {
                    Func<int, Components.AnimationConfig> CreateConfig(Direction direction) => index => new()
                    {
                        Direction = direction,
                        EasingFunction = "linear",
                        Duration = 5000,
                        Delay = 80
                    };
                    Console.WriteLine("SetSquaresPattern");
                    AnimationConfigs.SetDefaultConfig(clocks, CreateConfig(Direction.Anticlockwise), CreateConfig(Direction.Clockwise));
                }),

            AnimationPatternType.Flow => new InfiniteAnimationManager(JSRuntime, Clocks, clocks =>
                {
                    Func<int, Components.AnimationConfig> SetHourArmAnimationConfig = (index) => new Components.AnimationConfig
                    {
                        Direction = Direction.Anticlockwise,
                        EasingFunction = "linear",
                        Duration = 5000,
                        Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 40)
                    };
                    Console.WriteLine("SetFlowPattern");
                    AnimationConfigs.SetSpiralConfig(clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
                }),

            _ => throw new ArgumentOutOfRangeException(nameof(animationManager.AnimationPatternType), animationManager.AnimationPatternType, null),
        };
    }

    private IAnimationManager GetPatternAnimationManager()
    {
        return new PatternAnimationManager(JSRuntime, Clocks,
            clocks =>
            {
                Array values = Enum.GetValues(typeof(Direction));
                static Components.AnimationConfig SetHourArmAnimationConfig(int index, Direction direction) =>
                    new()
                    {
                        Direction = direction,
                        EasingFunction = "ease-in-out",
                        Duration = 5000,
                        Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 300)
                    };

                SetRandomClockConfigs(clocks, index => SetHourArmAnimationConfig(index, (Direction)values.GetValue(random.Next(values.Length))), index => SetHourArmAnimationConfig(index, (Direction)values.GetValue(random.Next(values.Length))));
                return SetRandomPattern(clocks);
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
                 Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 400)
             };

            SetRandomClockConfigs(Clocks,
            index => CreateArmAnimationConfig(index, Direction.Anticlockwise),
            index => CreateArmAnimationConfig(index, Direction.Clockwise));
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
        AnimationConfigType randomAnimationType = (AnimationConfigType)values.GetValue(random.Next(values.Length));

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
            case AnimationConfigType.CenterOut:
                static Components.AnimationConfig CreateArmAnimationConfig(Clock clock, int index, Direction direction) =>
                        new()
                        {
                            Direction = direction,
                            EasingFunction = "linear",
                            Duration = 7000,
                            Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 500)
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

    private AnimationPatternType SetRandomPattern(Dictionary<int, Clock> clocks)
    {
        Array values = Enum.GetValues(typeof(AnimationPatternType));
        AnimationPatternType randomAnimationPatternType = (AnimationPatternType)values.GetValue(random.Next(values.Length));

        switch (randomAnimationPatternType)
        {
            case AnimationPatternType.Flower:
                AnimationPatterns.SetFlowerPattern(clocks);
                break;
            case AnimationPatternType.Line:
                AnimationPatterns.SetLinePattern(clocks);
                break;
            case AnimationPatternType.Squares:
                AnimationPatterns.SetSquaresPattern(clocks);
                break;
            case AnimationPatternType.Flow:
                AnimationPatterns.SetFlowPattern(clocks);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(randomAnimationPatternType), randomAnimationPatternType, null);
        }
        return randomAnimationPatternType;
    }
}