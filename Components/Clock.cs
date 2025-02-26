namespace Time.Components;
using Time.Utils;
using Time.AnimationConfig;
public class Clock
{
    private ArmConfig _defaultFirstArmConfig = new ArmConfig
    {
        Direction = Direction.Clockwise,
        MaxSpeedDegrees = 1,
        Acceleration = 1,
        Deceleration = 1,

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
        FirstArm.CurrentState = 0;
        FirstArm.Config = firstArmConfig != null ? firstArmConfig : _defaultFirstArmConfig;
        SecondArm.CurrentState = 0;
        SecondArm.Config = secondArmConfig != null ? secondArmConfig : _defaultSecondArmConfig;
    }

    public void UpdateClockArmsConfig(ArmConfig firstArmConfig, ArmConfig secondArmConfig)
    {
        FirstArm.Config.Direction = firstArmConfig.Direction;
        FirstArm.Config.EasingFunction = firstArmConfig.EasingFunction;
        FirstArm.Config.MaxSpeedDegrees = firstArmConfig.MaxSpeedDegrees;
        SecondArm.Config.Direction = secondArmConfig.Direction;
        SecondArm.Config.EasingFunction = secondArmConfig.EasingFunction;
        SecondArm.Config.MaxSpeedDegrees = secondArmConfig.MaxSpeedDegrees;
    }

    public void ResetClock()
    {
        delayAnimation.Started = false;
        FirstArm.Config.EasingAnimation.ResetEasingAnimation();
        SecondArm.Config.EasingAnimation.ResetEasingAnimation();
    }

    public bool UpdateState(ArmState firstArmState, ArmState secondArmState, double timeElapsedMillisec, bool stopAtFinalState = true)
    {
        int firstArmFinalStateDegrees = 0;
        int secondArmFinalStateDegrees = 0;
        if (stopAtFinalState)
        {
            firstArmFinalStateDegrees = AnimationUtils.ArmStateToDegree(firstArmState);
            secondArmFinalStateDegrees = AnimationUtils.ArmStateToDegree(secondArmState);
            FirstArm.FinalState = firstArmFinalStateDegrees;
            SecondArm.FinalState = secondArmFinalStateDegrees;
        }
        if (!stopAtFinalState || (FirstArm.CurrentState != firstArmFinalStateDegrees))
        {
            if (delayAnimation.TimeIsUp(timeElapsedMillisec))
                UpdateArmState(FirstArm);
        }
        if (!stopAtFinalState || (SecondArm.CurrentState != secondArmFinalStateDegrees))
        {
            if (delayAnimation.TimeIsUp(timeElapsedMillisec))
                UpdateArmState(SecondArm);
        }
        return stopAtFinalState && (FirstArm.CurrentState == firstArmFinalStateDegrees) && (SecondArm.CurrentState ==
        secondArmFinalStateDegrees) ? true : false;
    }

    private void UpdateArmState(ClockArm arm)
    {
        arm.CurrentState = arm.Config.Direction == Direction.Clockwise ?
        arm.CurrentState + arm.Config.EasingAnimation.GetEasingValue() :
        arm.CurrentState - arm.Config.EasingAnimation.GetEasingValue();
    }
}

public class ClockArm
{
    private int _currentState;
    private int _finalState;
    public int CurrentState
    {
        get { return _currentState; }
        set { _currentState = value > 0 ? value % 360 : (value + 360) % 360; }
    }
    public int FinalState
    {
        get { return _finalState; }

        set
        {
            if (Math.Abs(_finalState) % 360 != value)
            {
                if (Config.Direction == Direction.Clockwise)
                {
                    var deltaDegrees = value - _finalState % 360;
                    if (deltaDegrees >= 0)
                        _finalState = _finalState + deltaDegrees;
                    else
                        _finalState = _finalState + 360 - _finalState % 360 + value;
                }
                if (Config.Direction == Direction.Anticlockwise)
                {
                    var convertedValueDegrees = -360 + value;
                    var convertedFinalState = _finalState <= 0 ? _finalState : -360 + _finalState % 360;

                    var deltaDegrees = Math.Abs(convertedValueDegrees) - Math.Abs(convertedFinalState) % 360;
                    if (deltaDegrees >= 0)
                        _finalState = _finalState - deltaDegrees;
                    else
                        _finalState = _finalState - (360 - Math.Abs(_finalState) % 360) + convertedValueDegrees;
                }
            }
        }
    }


    public ArmConfig Config { get; set; } = new ArmConfig();
}

public class ArmConfig
{
    public EasingAnimation EasingAnimation { get; private set; } = new EasingAnimation(EasingFunctions.Linear);
    public Direction Direction { get; set; } = Direction.Clockwise;
    public int MaxSpeedDegrees { get; set; } = 1;
    public int Acceleration { get; set; } = 1;
    public int Deceleration { get; set; } = 1;

    public Func<float, float> EasingFunction
    {
        get { return EasingAnimation.EasingFunction; }
        set { if (value is null) EasingAnimation.EasingFunction = EasingFunctions.Linear; else EasingAnimation.EasingFunction = value; }
    }
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
