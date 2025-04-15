using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;
public class AnimationOrchestrator(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks)
{
    private readonly IJSRuntime JSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> Clocks = clocks;
    private readonly Random random = new();
    private List<AnimationPatternType> remainingPatterns = Enum.GetValues(typeof(AnimationPatternType)).Cast<AnimationPatternType>().ToList();

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
                return (GetPatternAnimationManager(), 15000);
            }
            else if (animationManager.GetAnimationType() == AnimationType.Pattern)
            {
                return (GetInfiniteAnimationManager((PatternAnimationManager)animationManager), 25000);
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
            AnimationPatternType.Flower => new InfiniteAnimationManager(JSRuntime, Clocks, () =>
                {
                    static Components.AnimationConfig CreateArmAnimationConfig(Clock clock, int index, Direction direction) =>
                        new()
                        {
                            Direction = direction,
                            EasingFunction = "ease-in",
                            Duration = 7000,
                            Delay = AnimationConfigs.StaggeredAnimation(index, 0, 400)
                        };

                    static Components.AnimationConfig SetHourArmAnimationConfig(Clock clock, int index) =>
                        CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Clockwise : Direction.Anticlockwise);

                    static Components.AnimationConfig SetMinuteArmAnimationConfig(Clock clock, int index) =>
                        CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Anticlockwise : Direction.Clockwise);
                    Console.WriteLine("SetFlowerPattern");
                    AnimationConfigs.SetCenterOutConfig(Clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);
                }),

            AnimationPatternType.Line => new InfiniteAnimationManager(JSRuntime, Clocks, () =>
                {
                    Func<int, Components.AnimationConfig> CreateConfig(Direction direction) => index => new()
                    {
                        Direction = direction,
                        EasingFunction = "ease-in",
                        Duration = 5000,
                        Delay = AnimationConfigs.StaggeredAnimation(index, 0, 40)
                    };
                    Console.WriteLine("SetLinePattern");
                    AnimationConfigs.SetDefaultConfig(Clocks, CreateConfig(Direction.Anticlockwise), CreateConfig(Direction.Clockwise));
                }),

            AnimationPatternType.Diagonal => new InfiniteAnimationManager(JSRuntime, Clocks, () =>
               {
                   Components.AnimationConfig SetHourArmAnimationConfig(int index) => new()
                   {
                       Direction = Direction.Anticlockwise,
                       EasingFunction = "ease-in",
                       Duration = 5000,
                       Delay = AnimationConfigs.StaggeredAnimation(index, 0, 80)
                   };
                   Console.WriteLine("SetDiagonalPattern");
                   AnimationConfigs.SetDefaultConfig(Clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
               }),

            AnimationPatternType.Squares => new InfiniteAnimationManager(JSRuntime, Clocks, () =>
                {
                    Func<int, Components.AnimationConfig> CreateConfig(Direction direction) => index => new()
                    {
                        Direction = direction,
                        EasingFunction = "ease-in",
                        Duration = 5000,
                        Delay = 80
                    };
                    Console.WriteLine("SetSquaresPattern");
                    AnimationConfigs.SetDefaultConfig(Clocks, CreateConfig(Direction.Anticlockwise), CreateConfig(Direction.Clockwise));
                }),

            AnimationPatternType.Flow => new InfiniteAnimationManager(JSRuntime, Clocks, () =>
                {
                    Components.AnimationConfig SetHourArmAnimationConfig(int index) => new Components.AnimationConfig
                    {
                        Direction = Direction.Anticlockwise,
                        EasingFunction = "ease-in",
                        Duration = 5000,
                        Delay = AnimationConfigs.StaggeredAnimation(index, 0, 40)
                    };
                    Console.WriteLine("SetFlowPattern");
                    AnimationConfigs.SetSpiralConfig(Clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
                }),

            AnimationPatternType.Numbers => new InfiniteAnimationManager(JSRuntime, Clocks, () =>
            {
                Components.AnimationConfig SetHourArmAnimationConfig(int index) => new Components.AnimationConfig
                {
                    Direction = Direction.Anticlockwise,
                    EasingFunction = "ease-in",
                    Duration = 7000,
                    Delay = 0
                };
                Console.WriteLine("SetNumbersPattern");
                AnimationConfigs.SetDefaultConfig(Clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
            }),

            _ => throw new ArgumentOutOfRangeException(nameof(animationManager.AnimationPatternType), animationManager.AnimationPatternType, null),
        };
    }

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
                return SetRandomPattern(Clocks);
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
                            EasingFunction = "linear",
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

    private AnimationPatternType SetRandomPattern(Dictionary<int, Clock> clocks)
    {
        if (remainingPatterns.Count == 0)
        {
            remainingPatterns = Enum.GetValues(typeof(AnimationPatternType)).Cast<AnimationPatternType>().ToList();
        }

        AnimationPatternType randomAnimationPatternType = remainingPatterns[random.Next(remainingPatterns.Count)];
        remainingPatterns.Remove(randomAnimationPatternType);

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
            case AnimationPatternType.Diagonal:
                AnimationPatterns.SetDiagonalPattern(clocks);
                break;
            case AnimationPatternType.Numbers:
                var time = DateTime.Now;
                var hoursFirstDigit = time.Hour / 10;
                var hoursSecondDigit = time.Hour % 10;
                var minuteFirstDigit = time.Minute / 10;
                var minuteSecondDigit = time.Minute % 10;
                AnimationPatterns.SetNumbersPattern(clocks, hoursFirstDigit, hoursSecondDigit, minuteFirstDigit, minuteSecondDigit, 90);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(randomAnimationPatternType), randomAnimationPatternType, null);
        }
        return randomAnimationPatternType;
    }
}