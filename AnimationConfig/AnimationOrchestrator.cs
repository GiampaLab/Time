using Microsoft.JSInterop;
using Time.AnimationEngine;
using Time.Components;

namespace Time.AnimationConfig;
public class AnimationOrchestrator(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks)
{
    private readonly IJSRuntime JSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> Clocks = clocks;

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
                return (GetInfiniteAnimationManager(), 20000);
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

    private IAnimationManager GetInfiniteAnimationManager()
    {
        return new InfiniteAnimationManager(JSRuntime, Clocks,
            clocks =>
            {
                static Components.AnimationConfig CreateArmAnimationConfig(Clock clock, int index, Direction direction) =>
                    new()
                    {
                        Direction = direction,
                        EasingFunction = "linear",
                        Duration = AnimationConfigs.StaggeredAnimation(false, index, 7000, 0),
                        Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 500)
                    };

                static Components.AnimationConfig SetHourArmAnimationConfig(Clock clock, int index) =>
                    CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Clockwise : Direction.Anticlockwise);

                static Components.AnimationConfig SetMinuteArmAnimationConfig(Clock clock, int index) =>
                    CreateArmAnimationConfig(clock, index, clock.Id <= 12 ? Direction.Anticlockwise : Direction.Clockwise);

                AnimationConfigs.SetCenterOutConfig(clocks, SetHourArmAnimationConfig, SetMinuteArmAnimationConfig);
            });
    }

    private IAnimationManager GetPatternAnimationManager()
    {
        return new PatternAnimationManager(JSRuntime, Clocks,
            clocks =>
            {
                static Components.AnimationConfig SetHourArmAnimationConfig(int index) =>
                    new()
                    {
                        Direction = Direction.Anticlockwise,
                        EasingFunction = "ease-in-out",
                        Duration = AnimationConfigs.StaggeredAnimation(false, index, 4000, 0),
                        Delay = AnimationConfigs.StaggeredAnimation(true, index, 0, 120)
                    };

                AnimationConfigs.SetSpiralConfig(clocks, SetHourArmAnimationConfig, SetHourArmAnimationConfig);
                AnimationPatterns.SetFlowerPattern(clocks);
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

    private static void SetRandomClockConfigs(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        Array values = Enum.GetValues(typeof(AnimationConfigType));
        Random random = new();
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
            case AnimationConfigType.CenterOut:
                AnimationConfigs.SetCenterOutConfig(clocks,
                    (clock, index) => selectFirstArmConfig(index),
                    (clock, index) => selectSecondArmConfig(index));
                break;
            case AnimationConfigType.Spiral:
                AnimationConfigs.SetSpiralConfig(clocks, selectFirstArmConfig, selectSecondArmConfig);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(randomAnimationType), randomAnimationType, null);
        }
    }
}