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
    public Clock(int Id, AnimationConfig? firstArmConfig = null, AnimationConfig? secondArmConfig = null)
    {
        this.Id = Id;
        FirstArm.CurrentState = 0;
        FirstArm.Config = firstArmConfig ?? _defaultFirstArmConfig;
        SecondArm.CurrentState = 0;
        SecondArm.Config = secondArmConfig ?? _defaultSecondArmConfig;
    }

    public void UpdateClockArmsConfig(AnimationConfig firstArmConfig, AnimationConfig secondArmConfig, ElementReference hourReference, ElementReference minuteReference)
    {
        FirstArm.Config.Direction = firstArmConfig.Direction;
        FirstArm.Config.EasingFunction = firstArmConfig.EasingFunction;
        FirstArm.Config.ElementReference = hourReference;
        FirstArm.Config.Duration = firstArmConfig.Duration;
        FirstArm.Config.Delay = firstArmConfig.Delay;
        SecondArm.Config.Direction = secondArmConfig.Direction;
        SecondArm.Config.EasingFunction = secondArmConfig.EasingFunction;
        SecondArm.Config.ElementReference = minuteReference;
        SecondArm.Config.Duration = secondArmConfig.Duration;
        SecondArm.Config.Delay = secondArmConfig.Delay;
    }

    public void UpdateState(ArmState firstArmState, ArmState secondArmState)
    {
        var firstArmFinalStateDegrees = AnimationUtils.ArmStateToDegree(firstArmState);
        var secondArmFinalStateDegrees = AnimationUtils.ArmStateToDegree(secondArmState);
        FirstArm.FinalState = firstArmFinalStateDegrees;
        SecondArm.FinalState = secondArmFinalStateDegrees;
    }
}

public class ClockArm
{
    private int _currentState;
    public int CurrentState
    {
        get { return _currentState; }
        set { _currentState = value > 0 ? value % 360 : (value + 360) % 360; }
    }
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
