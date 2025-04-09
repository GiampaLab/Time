using Time.Components;

namespace Time.AnimationConfig;
public class AnimationPatterns
{
    public static void SetNumbersPattern(Dictionary<int, Clock> clocks, int hoursFirstDigit, int hoursSecondDigit, int minuteFirstDigit, int minuteSecondDigit)
    {
        SetNumberPattern(hoursFirstDigit, [1, 2, 3, 4, 5, 6], clocks);
        SetNumberPattern(hoursSecondDigit, [7, 8, 9, 10, 11, 12], clocks);
        SetNumberPattern(minuteFirstDigit, [13, 14, 15, 16, 17, 18], clocks);
        SetNumberPattern(minuteSecondDigit, [19, 20, 21, 22, 23, 24], clocks);
    }

    private static void SetNumberPattern(int number, List<int> clockIndexes, Dictionary<int, Clock> clocks)
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

    public static void SetSquaresPattern(Dictionary<int, Clock> clocks)
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

    public static void SetLinePattern(Dictionary<int, Clock> clocks)
    {
        for (var i = 0; i < 24; i++)
        {
            clocks[i + 1].UpdateState(ArmState.Zero, ArmState.Six);
        }
    }

    public static void SetFlowPattern(Dictionary<int, Clock> clocks)
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

    public static void SetFlowerPattern(Dictionary<int, Clock> clocks)
    {
        clocks[1].UpdateState(ArmState.Zero, ArmState.Six, 10, 10);
        clocks[4].UpdateState(ArmState.Zero, ArmState.Six, 20, 15);
        clocks[7].UpdateState(ArmState.Zero, ArmState.Six, 30, 20);
        clocks[10].UpdateState(ArmState.Zero, ArmState.Six, 70, 60);

        clocks[13].UpdateState(ArmState.Zero, ArmState.Six, -70, -60);
        clocks[16].UpdateState(ArmState.Zero, ArmState.Six, -30, -20);
        clocks[19].UpdateState(ArmState.Zero, ArmState.Six, -20, -15);
        clocks[22].UpdateState(ArmState.Zero, ArmState.Six, -10, -10);

        clocks[2].UpdateState(ArmState.Zero, ArmState.Six, 5, -5);
        clocks[5].UpdateState(ArmState.Zero, ArmState.Six, 8, -8);
        clocks[8].UpdateState(ArmState.Zero, ArmState.Six, 10, -10);
        clocks[11].UpdateState(ArmState.Zero, ArmState.Six, 30, -30);

        clocks[14].UpdateState(ArmState.Zero, ArmState.Six, -30, 30);
        clocks[17].UpdateState(ArmState.Zero, ArmState.Six, -10, 10);
        clocks[20].UpdateState(ArmState.Zero, ArmState.Six, -8, 8);
        clocks[23].UpdateState(ArmState.Zero, ArmState.Six, -5, 5);

        clocks[3].UpdateState(ArmState.Zero, ArmState.Six, -10, -10);
        clocks[6].UpdateState(ArmState.Zero, ArmState.Six, -15, -20);
        clocks[9].UpdateState(ArmState.Zero, ArmState.Six, -20, -30);
        clocks[12].UpdateState(ArmState.Zero, ArmState.Six, -60, -70);

        clocks[15].UpdateState(ArmState.Zero, ArmState.Six, 60, 70);
        clocks[18].UpdateState(ArmState.Zero, ArmState.Six, 20, 30);
        clocks[21].UpdateState(ArmState.Zero, ArmState.Six, 15, 20);
        clocks[24].UpdateState(ArmState.Zero, ArmState.Six, 10, 10);
    }
}

public enum AnimationPatternType
{
    Squares,
    Line,
    Flow,
    Flower
}