namespace Time.Components;
using Time.AnimationConfig;
public class Clock
{
    private ArmConfig _defaultFirstArmConfig = new ArmConfig
    {
        Direction = Direction.Clockwise,
        MaxSpeedDegrees = 1,
        Acceleration = 1,
        Deceleration = 1
    };
    private ArmConfig _defaultSecondArmConfig = new ArmConfig
    {
        Direction = Direction.Clockwise,
        MaxSpeedDegrees = 1,
        Acceleration = 1,
        Deceleration = 1
    };
    public int Id { get; private set; }
    public ClockArm FirstArm { get; private set; } = new ClockArm();
    public ClockArm SecondArm { get; private set; } = new ClockArm();
    public DelayAnimation delayAnimation { get; private set; } = new DelayAnimation();
    public Clock(int Id, ArmConfig? firstArmConfig = null, ArmConfig? secondArmConfig = null)
    {
        this.Id = Id;
        FirstArm.CurrentState = 0.0;
        FirstArm.Config = firstArmConfig != null ? firstArmConfig : _defaultFirstArmConfig;
        SecondArm.CurrentState = 0.0;
        SecondArm.Config = secondArmConfig != null ? secondArmConfig : _defaultSecondArmConfig;
    }

    public void UpdateClockArmsConfig(ArmConfig firstArmConfig, ArmConfig secondArmConfig)
    {
        FirstArm.Config = firstArmConfig;
        SecondArm.Config = secondArmConfig;
    }

    public bool UpdateState(ArmState firstArmState, ArmState secondArmState, double firstArmStateDeltaDegrees = 0, double
    secondArmStateDeltaDegrees = 0, bool stopAtFinalState = true)
    {
        var firstArmFinalStateDegrees = Math.Round(AnimationUtils.ArmStateToDegree(firstArmState) + firstArmStateDeltaDegrees, 2);
        var secondArmFinalStateDegrees = Math.Round(AnimationUtils.ArmStateToDegree(secondArmState) + secondArmStateDeltaDegrees, 2);
        if (!stopAtFinalState || (FirstArm.CurrentState != firstArmFinalStateDegrees))
        {
            UpdateArmState(FirstArm);
        }
        if (!stopAtFinalState || (SecondArm.CurrentState != secondArmFinalStateDegrees))
        {
            UpdateArmState(SecondArm);
        }
        return stopAtFinalState && (FirstArm.CurrentState == firstArmFinalStateDegrees) && (SecondArm.CurrentState ==
        secondArmFinalStateDegrees) ? true : false;
    }

    private void UpdateArmState(ClockArm arm)
    {
        arm.CurrentState = arm.Config.Direction == Direction.Clockwise ?
        Math.Round(arm.CurrentState + arm.Config.MaxSpeedDegrees, 2) :
        Math.Round(arm.CurrentState - arm.Config.MaxSpeedDegrees, 2);
    }
}

public class ClockArm
{
    private double _currentState;
    public double CurrentState
    {
        get { return _currentState; }
        set { _currentState = value > 0 ? value % 360.0 : (value + 360.0) % 360.0; }
    }
    public ArmConfig Config { get; set; } = new ArmConfig();
}

public class ArmConfig
{
    public Direction Direction { get; set; } = Direction.Clockwise;
    public double MaxSpeedDegrees { get; set; } = 1;
    public int Acceleration { get; set; } = 1;
    public int Deceleration { get; set; } = 1;
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
