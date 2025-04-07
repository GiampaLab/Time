using Time.Components;

namespace Time.AnimationConfig;
public class AnimationConfigs
{
    public static int StaggeredAnimation(bool staggered, int index, int milliseconds, int staggeredMilliseconds)
    {
        return staggered ? milliseconds + ((index % 2) == 1 ? (index - 1) * staggeredMilliseconds : index * staggeredMilliseconds) : milliseconds;
    }

    public static void SetStaticClocksAnimationConfigs(Dictionary<int, Clock> clocks, Components.AnimationConfig firstArmConfig, Components.AnimationConfig secondArmConfig)
    {
        for (var i = 0; i < 24; i++)
        {
            //config is the same for all clocks
            clocks[i + 1].UpdateClockArmsConfig(firstArmConfig, secondArmConfig);
        }
    }

    public static void SetClocksAnimationConfigs(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        for (var i = 0; i < 24; i++)
        {
            clocks[i + 1].UpdateClockArmsConfig(selectFirstArmConfig(i), selectSecondArmConfig(i));
        }
    }

    public static void SetReverseClocksConfigs(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        var j = 0;
        for (var i = 23; i >= 0; i--)
        {
            clocks[i + 1].UpdateClockArmsConfig(selectFirstArmConfig(j), selectSecondArmConfig(j));
            j++;
        }
    }

    public static void SetClocksConfigsByRow(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        for (var i = 0; i < 3; i++)
        {
            clocks[i + 1].UpdateClockArmsConfig(selectFirstArmConfig(0 + i * 8), selectSecondArmConfig(0 + i * 8));
            clocks[i + 4].UpdateClockArmsConfig(selectFirstArmConfig(1 + i * 8), selectSecondArmConfig(1 + i * 8));
            clocks[i + 7].UpdateClockArmsConfig(selectFirstArmConfig(2 + i * 8), selectSecondArmConfig(2 + i * 8));
            clocks[i + 10].UpdateClockArmsConfig(selectFirstArmConfig(3 + i * 8), selectSecondArmConfig(3 + i * 8));
            clocks[i + 13].UpdateClockArmsConfig(selectFirstArmConfig(4 + i * 8), selectSecondArmConfig(4 + i * 8));
            clocks[i + 16].UpdateClockArmsConfig(selectFirstArmConfig(5 + i * 8), selectSecondArmConfig(5 + i * 8));
            clocks[i + 19].UpdateClockArmsConfig(selectFirstArmConfig(6 + i * 8), selectSecondArmConfig(6 + i * 8));
            clocks[i + 22].UpdateClockArmsConfig(selectFirstArmConfig(7 + i * 8), selectSecondArmConfig(7 + i * 8));
        }
    }

    public static void SetClocksConfigsSpiral(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        clocks[14].UpdateClockArmsConfig(selectFirstArmConfig(0), selectSecondArmConfig(0));
        clocks[15].UpdateClockArmsConfig(selectFirstArmConfig(1), selectSecondArmConfig(1));
        clocks[12].UpdateClockArmsConfig(selectFirstArmConfig(2), selectSecondArmConfig(2));
        clocks[11].UpdateClockArmsConfig(selectFirstArmConfig(3), selectSecondArmConfig(3));
        clocks[10].UpdateClockArmsConfig(selectFirstArmConfig(4), selectSecondArmConfig(4));
        clocks[13].UpdateClockArmsConfig(selectFirstArmConfig(5), selectSecondArmConfig(5));
        clocks[16].UpdateClockArmsConfig(selectFirstArmConfig(6), selectSecondArmConfig(6));
        clocks[17].UpdateClockArmsConfig(selectFirstArmConfig(7), selectSecondArmConfig(7));
        clocks[18].UpdateClockArmsConfig(selectFirstArmConfig(8), selectSecondArmConfig(8));
        clocks[9].UpdateClockArmsConfig(selectFirstArmConfig(9), selectSecondArmConfig(9));
        clocks[8].UpdateClockArmsConfig(selectFirstArmConfig(10), selectSecondArmConfig(10));
        clocks[7].UpdateClockArmsConfig(selectFirstArmConfig(11), selectSecondArmConfig(11));
        clocks[19].UpdateClockArmsConfig(selectFirstArmConfig(12), selectSecondArmConfig(12));
        clocks[20].UpdateClockArmsConfig(selectFirstArmConfig(13), selectSecondArmConfig(13));
        clocks[21].UpdateClockArmsConfig(selectFirstArmConfig(14), selectSecondArmConfig(14));
        clocks[6].UpdateClockArmsConfig(selectFirstArmConfig(15), selectSecondArmConfig(15));
        clocks[5].UpdateClockArmsConfig(selectFirstArmConfig(16), selectSecondArmConfig(16));
        clocks[4].UpdateClockArmsConfig(selectFirstArmConfig(17), selectSecondArmConfig(17));
        clocks[22].UpdateClockArmsConfig(selectFirstArmConfig(18), selectSecondArmConfig(18));
        clocks[23].UpdateClockArmsConfig(selectFirstArmConfig(19), selectSecondArmConfig(19));
        clocks[24].UpdateClockArmsConfig(selectFirstArmConfig(20), selectSecondArmConfig(20));
        clocks[3].UpdateClockArmsConfig(selectFirstArmConfig(21), selectSecondArmConfig(21));
        clocks[2].UpdateClockArmsConfig(selectFirstArmConfig(22), selectSecondArmConfig(22));
        clocks[1].UpdateClockArmsConfig(selectFirstArmConfig(23), selectSecondArmConfig(23));
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

    public static void SetNextPatternAnimationStatus(Dictionary<int, Clock> clocks)
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

    public static void SetNextWaveAnimationStatus(Dictionary<int, Clock> clocks)
    {
        for (var i = 0; i < 24; i++)
        {
            clocks[i + 1].UpdateState(ArmState.Zero, ArmState.Six);
        }
    }

    public static void SetFlowAnimationStatus(Dictionary<int, Clock> clocks)
    {
        clocks[1].UpdateState(ArmState.Three, ArmState.Three, 10, 10);
        clocks[4].UpdateState(ArmState.Three, ArmState.Three, 20, 20);
        clocks[7].UpdateState(ArmState.Three, ArmState.Three, 30, 30);
        clocks[10].UpdateState(ArmState.Three, ArmState.Three, 40, 40);

        clocks[13].UpdateState(ArmState.Nine, ArmState.Nine, -40, -40);
        clocks[16].UpdateState(ArmState.Nine, ArmState.Nine, -30, -30);
        clocks[19].UpdateState(ArmState.Nine, ArmState.Nine, -20, -20);
        clocks[22].UpdateState(ArmState.Nine, ArmState.Nine, -10, -10);

        clocks[2].UpdateState(ArmState.Three, ArmState.Three);
        clocks[5].UpdateState(ArmState.Three, ArmState.Three);
        clocks[8].UpdateState(ArmState.Three, ArmState.Three);
        clocks[11].UpdateState(ArmState.Three, ArmState.Three);

        clocks[14].UpdateState(ArmState.Nine, ArmState.Nine);
        clocks[17].UpdateState(ArmState.Nine, ArmState.Nine);
        clocks[20].UpdateState(ArmState.Nine, ArmState.Nine);
        clocks[23].UpdateState(ArmState.Nine, ArmState.Nine);

        clocks[3].UpdateState(ArmState.Three, ArmState.Three, -10, -10);
        clocks[6].UpdateState(ArmState.Three, ArmState.Three, -20, -20);
        clocks[9].UpdateState(ArmState.Three, ArmState.Three, -30, -30);
        clocks[12].UpdateState(ArmState.Three, ArmState.Three, -40, -40);

        clocks[15].UpdateState(ArmState.Nine, ArmState.Nine, 40, 40);
        clocks[18].UpdateState(ArmState.Nine, ArmState.Nine, 30, 30);
        clocks[21].UpdateState(ArmState.Nine, ArmState.Nine, 20, 20);
        clocks[24].UpdateState(ArmState.Nine, ArmState.Nine, 10, 10);
    }
}