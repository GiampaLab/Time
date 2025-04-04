using Time.Components;

namespace Time.AnimationConfig;
public class AnimationConfigs
{
    public static void SetClocksAnimationConfigs(Dictionary<int, Clock> clocks, Func<Clock, int, Components.AnimationConfig> selectFirstArmConfig, Func<Clock, int, Components.AnimationConfig> selectSecondArmConfig)
    {
        for (var i = 0; i < 24; i++)
        {
            clocks[i + 1].UpdateClockArmsConfig(selectFirstArmConfig(clocks[i + 1], i), selectSecondArmConfig(clocks[i + 1], i));
        }
    }

    public static void SetReverseClocksConfigs(Dictionary<int, Clock> clocks, Func<Clock, int, Components.AnimationConfig> selectFirstArmConfig, Func<Clock, int, Components.AnimationConfig> selectSecondArmConfig)
    {
        var j = 0;
        for (var i = 23; i >= 0; i--)
        {
            clocks[i + 1].UpdateClockArmsConfig(selectFirstArmConfig(clocks[i + 1], j), selectSecondArmConfig(clocks[i + 1], j));
            j++;
        }
    }

    public static void SetNextNumbersAnimationStatus(Dictionary<int, Clock> clocks, int hoursFirstDigit, int hoursSecondDigit, int minuteFirstDigit, int minuteSecondDigit)
    {
        SetNumber(hoursFirstDigit, new List<int> { 1, 2, 3, 4, 5, 6 }, clocks);
        SetNumber(hoursSecondDigit, new List<int> { 7, 8, 9, 10, 11, 12 }, clocks);
        SetNumber(minuteFirstDigit, new List<int> { 13, 14, 15, 16, 17, 18 }, clocks);
        SetNumber(minuteSecondDigit, new List<int> { 19, 20, 21, 22, 23, 24 }, clocks);
    }

    private static void SetNumber(int number, List<int> clockIndexes, Dictionary<int, Clock> clocks)
    {
        switch (number)
        {
            case 0:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six);
                clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Six);
                clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine);
                break;

            case 1:
                clocks[clockIndexes[0]].UpdateState(ArmState.None, ArmState.None);
                clocks[clockIndexes[1]].UpdateState(ArmState.None, ArmState.None);
                clocks[clockIndexes[2]].UpdateState(ArmState.None, ArmState.None);
                clocks[clockIndexes[3]].UpdateState(ArmState.Six, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Zero);
                break;

            case 2:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Three);
                clocks[clockIndexes[1]].UpdateState(ArmState.Three, ArmState.Six);
                clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Nine);
                clocks[clockIndexes[5]].UpdateState(ArmState.Nine, ArmState.Nine);
                break;

            case 3:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Three);
                clocks[clockIndexes[1]].UpdateState(ArmState.Three, ArmState.Three);
                clocks[clockIndexes[2]].UpdateState(ArmState.Three, ArmState.Three);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine);
                break;

            case 4:
                clocks[clockIndexes[0]].UpdateState(ArmState.Six, ArmState.Six);
                clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[2]].UpdateState(ArmState.None, ArmState.None);
                clocks[clockIndexes[3]].UpdateState(ArmState.Six, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Nine, ArmState.Zero);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Zero);
                break;

            case 5:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six);
                clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[2]].UpdateState(ArmState.Three, ArmState.Three);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Nine);
                clocks[clockIndexes[4]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine);
                break;

            case 6:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six);
                clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Six);
                clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Nine);
                clocks[clockIndexes[4]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine);
                break;

            case 7:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Three);
                clocks[clockIndexes[1]].UpdateState(ArmState.None, ArmState.None);
                clocks[clockIndexes[2]].UpdateState(ArmState.None, ArmState.None);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Zero);
                break;

            case 8:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six);
                clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Nine);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine);
                break;

            case 9:
                clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six);
                clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three);
                clocks[clockIndexes[2]].UpdateState(ArmState.Three, ArmState.Three);
                clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six);
                clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six);
                clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine);
                break;
            default:
                break;
        }
    }

    public static void SetNextPatternAnimationStatus(Dictionary<int, Clock> clocks, bool stopAtFinalState = true)
    {
        for (var i = 0; i < 4; i++)
        {
            var j = 6 * i;
            clocks[j + 1].UpdateState(ArmState.Three, ArmState.Six);
            clocks[j + 2].UpdateState(ArmState.Three, ArmState.Three);
            clocks[j + 3].UpdateState(ArmState.Zero, ArmState.Three);
            clocks[j + 4].UpdateState(ArmState.Six, ArmState.Nine);
            clocks[j + 5].UpdateState(ArmState.Nine, ArmState.Nine);
            clocks[j + 6].UpdateState(ArmState.Nine, ArmState.Zero);
        }
    }

    public static void SetNextWaveAnimationStatus(Dictionary<int, Clock> clocks, bool stopAtFinalState = true)
    {
        for (var i = 0; i < 24; i++)
        {
            clocks[i + 1].UpdateState(ArmState.Zero, ArmState.Six);
        }
    }
}