namespace Time.Components;
using Time.Utils;
using Time.AnimationConfig;
using Microsoft.AspNetCore.Components;

public class Clock
{
    private ArmConfig _defaultFirstArmConfig = new ArmConfig
    {
        Direction = Direction.Clockwise,
    };
    private ArmConfig _defaultSecondArmConfig = new ArmConfig
    {
        Direction = Direction.Clockwise,
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

    public void UpdateClockArmsConfig(ArmConfig firstArmConfig, ArmConfig secondArmConfig, ElementReference hourReference, ElementReference minuteReference)
    {
        FirstArm.Config.Direction = firstArmConfig.Direction;
        FirstArm.Config.EasingFunction = firstArmConfig.EasingFunction;
        FirstArm.Config.ElementReference = hourReference;
        SecondArm.Config.Direction = secondArmConfig.Direction;
        SecondArm.Config.EasingFunction = secondArmConfig.EasingFunction;
        SecondArm.Config.ElementReference = minuteReference;
    }

    public void ResetClock()
    {
        delayAnimation.Started = false;
        FirstArm.Config.EasingAnimation.ResetEasingAnimation();
        SecondArm.Config.EasingAnimation.ResetEasingAnimation();
    }

    public void UpdateState(ArmState firstArmState, ArmState secondArmState, bool stopAtFinalState = true)
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

        set
        {
            if (Math.Abs(Config.State) % 360 != value)
            {
                if (Config.Direction == Direction.Clockwise)
                {
                    var deltaDegrees = value - Config.State % 360;
                    if (deltaDegrees >= 0)
                        Config.State = Config.State + deltaDegrees;
                    else
                        Config.State = Config.State + 360 - Config.State % 360 + value;
                }
                if (Config.Direction == Direction.Anticlockwise)
                {
                    var convertedValueDegrees = -360 + value;
                    var convertedFinalState = Config.State <= 0 ? Config.State : -360 + Config.State % 360;

                    var deltaDegrees = Math.Abs(convertedValueDegrees) - Math.Abs(convertedFinalState) % 360;
                    if (deltaDegrees >= 0)
                        Config.State = Config.State - deltaDegrees;
                    else
                        Config.State = Config.State - (360 - Math.Abs(Config.State) % 360) + convertedValueDegrees;
                }
            }
        }
    }


    public ArmConfig Config { get; set; } = new ArmConfig();
}

public class ArmConfig
{
    public int State { get; internal set; }
    public EasingAnimation EasingAnimation { get; private set; } = new EasingAnimation(EasingFunctions.Linear);
    public Direction Direction { get; set; } = Direction.Clockwise;
    public string EasingFunction { get; set; } = "linear";
    public ElementReference ElementReference { get; internal set; }
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
