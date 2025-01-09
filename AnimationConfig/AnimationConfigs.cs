namespace Time.AnimationConfig;
using Time.Components;
public class AnimationConfigs
{
    public static void SetClocksConfigs(Dictionary<int, Clock> clocks, ArmConfig firstArmConfig, ArmConfig secondArmConfig, int progressiveDelay)
    {
        for (var i = 0; i < 24; i++)
        {
            clocks[i + 1].UpdateClockArmsConfig(firstArmConfig, secondArmConfig);
            clocks[i + 1].ResetClock();
            clocks[i + 1].delayAnimation.DelayMillisec = progressiveDelay * (i + 1);
        }
    }

    public static bool SetNextNumbersAnimationStatus(Dictionary<int, Clock> clocks, double timeElapsedMillisec)
    {
        var timeSetupCompleted = true;

        var time = DateTime.Now;

        var hoursFirstDigit = time.Hour / 10;

        var hoursSecondDigit = time.Hour % 10;

        var minuteFirstDigit = time.Minute / 10;

        var minuteSecondDigit = time.Minute % 10;

        var completed = SetNumber(hoursFirstDigit, new List<int> { 1, 2, 3, 4, 5, 6 }, clocks, timeElapsedMillisec);
        timeSetupCompleted &= completed;
        completed = SetNumber(hoursSecondDigit, new List<int> { 7, 8, 9, 10, 11, 12 }, clocks, timeElapsedMillisec);
        timeSetupCompleted &= completed;
        completed = SetNumber(minuteFirstDigit, new List<int> { 13, 14, 15, 16, 17, 18 }, clocks, timeElapsedMillisec);
        timeSetupCompleted &= completed;
        completed = SetNumber(minuteSecondDigit, new List<int> { 19, 20, 21, 22, 23, 24 }, clocks, timeElapsedMillisec);
        timeSetupCompleted &= completed;

        return timeSetupCompleted;
    }

    private static bool SetNumber(int number, List<int> clockIndexes, Dictionary<int, Clock> clocks, double timeElapsedMillisec)
    {
        var completed = true;
        var result = false;
        switch (number)
        {
            case 0:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 1:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.None, ArmState.None, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.None, ArmState.None, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.None, ArmState.None, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Six, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Zero, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 2:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Three, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Nine, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 3:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 4:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Six, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.None, ArmState.None, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Six, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Nine, ArmState.Zero, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Zero, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 5:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 6:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 7:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.None, ArmState.None, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.None, ArmState.None, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Zero, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 8:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                break;

            case 9:
                result = clocks[clockIndexes[0]].UpdateState(ArmState.Three, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[1]].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[2]].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[3]].UpdateState(ArmState.Nine, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[4]].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec);
                completed = completed && result;
                result = clocks[clockIndexes[5]].UpdateState(ArmState.Zero, ArmState.Nine, timeElapsedMillisec);
                completed = completed && result;
                break;
            default:
                break;
        }
        return completed;
    }

    public static bool SetNextPatternAnimationStatus(Dictionary<int, Clock> clocks, double timeElapsedMillisec, bool stopAtFinalState = true)
    {
        var completed = true;
        for (var i = 0; i < 4; i++)
        {
            var j = 6 * i;
            var result = clocks[j + 1].UpdateState(ArmState.Three, ArmState.Six, timeElapsedMillisec, 0, 0, stopAtFinalState);
            completed = completed && result;
            result = clocks[j + 2].UpdateState(ArmState.Three, ArmState.Three, timeElapsedMillisec, 0, 0, stopAtFinalState);
            completed = completed && result;
            result = clocks[j + 3].UpdateState(ArmState.Zero, ArmState.Three, timeElapsedMillisec, 0, 0, stopAtFinalState);
            completed = completed && result;
            result = clocks[j + 4].UpdateState(ArmState.Six, ArmState.Nine, timeElapsedMillisec, 0, 0, stopAtFinalState);
            completed = completed && result;
            result = clocks[j + 5].UpdateState(ArmState.Nine, ArmState.Nine, timeElapsedMillisec, 0, 0, stopAtFinalState);
            completed = completed && result;
            result = clocks[j + 6].UpdateState(ArmState.Nine, ArmState.Zero, timeElapsedMillisec, 0, 0, stopAtFinalState);
            completed = completed && result;
        }
        return completed;
    }

    public static bool SetNextWaveAnimationStatus(Dictionary<int, Clock> clocks, double timeElapsedMillisec, bool stopAtFinalState = true)
    {
        var completed = true;
        for (var i = 0; i < 24; i++)
        {
            var result = clocks[i + 1].UpdateState(ArmState.Zero, ArmState.Six, timeElapsedMillisec, 0, 0, stopAtFinalState);
            completed = completed && result;
        }
        return completed;
    }
}