namespace Time.Components;
using Time.AnimationConfig;
using Microsoft.AspNetCore.Components;

public class Clock
{
    private AnimationConfig _defaultFirstArmConfig = new()
    {
        Direction = Direction.Clockwise,
    };
    private AnimationConfig _defaultSecondArmConfig = new()
    {
        Direction = Direction.Clockwise,
    };
    public int Id { get; private set; }
    public ClockArm FirstArm { get; private set; } = new();
    public ClockArm SecondArm { get; private set; } = new();
    public Clock(int Id, ElementReference hourReference, ElementReference minuteReference, AnimationConfig? firstArmConfig = null, AnimationConfig? secondArmConfig = null)
    {
        this.Id = Id;
        FirstArm.Config = firstArmConfig ?? _defaultFirstArmConfig;
        FirstArm.Config.ElementReference = hourReference;
        SecondArm.Config = secondArmConfig ?? _defaultSecondArmConfig;
        SecondArm.Config.ElementReference = minuteReference;
    }

    public void UpdateClockArmsConfig(AnimationConfig firstArmConfig, AnimationConfig secondArmConfig)
    {
        FirstArm.Config.Direction = firstArmConfig.Direction;
        FirstArm.Config.EasingFunction = firstArmConfig.EasingFunction;
        FirstArm.Config.Duration = firstArmConfig.Duration;
        FirstArm.Config.Delay = firstArmConfig.Delay;
        SecondArm.Config.Direction = secondArmConfig.Direction;
        SecondArm.Config.EasingFunction = secondArmConfig.EasingFunction;
        SecondArm.Config.Duration = secondArmConfig.Duration;
        SecondArm.Config.Delay = secondArmConfig.Delay;
    }

    public void UpdateState(ArmState firstArmState, ArmState secondArmState, int firstArmStateDeltaDegrees = 0, int secondArmStateDeltaDegrees = 0)
    {
        FirstArm.FinalState = firstArmStateDeltaDegrees + AnimationUtils.ArmStateToDegree(firstArmState);
        SecondArm.FinalState = secondArmStateDeltaDegrees + AnimationUtils.ArmStateToDegree(secondArmState);
    }
}

public class ClockArm
{
    public int FinalState
    {
        get { return Config.State; }

        set { Config.State = value; }
    }

    public AnimationConfig Config { get; set; } = new AnimationConfig();
}

public class AnimationConfig
{
    public int State { get; internal set; }
    public Direction Direction { get; set; } = Direction.Clockwise;
    public string EasingFunction { get; set; } = "linear";
    public ElementReference ElementReference { get; internal set; }
    public int Duration { get; set; }
    public int Delay { get; set; }
}

public enum Direction
{
    Clockwise,
    Anticlockwise
}

public enum ArmState
{
    Zero,
    Three,
    Six,
    Nine,
    None,
    HPOne
}
