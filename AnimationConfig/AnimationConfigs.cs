using Time.Components;

namespace Time.AnimationConfig;
public class AnimationConfigs
{
    public static int StaggeredAnimation(bool staggered, int index, int milliseconds, int staggeredMilliseconds)
    {
        return staggered ? milliseconds + ((index % 2) == 1 ? (index - 1) * staggeredMilliseconds : index * staggeredMilliseconds) : milliseconds;
    }

    public static void SetStaticConfig(Dictionary<int, Clock> clocks, Components.AnimationConfig firstArmConfig, Components.AnimationConfig secondArmConfig)
    {
        for (var i = 0; i < 24; i++)
        {
            //config is the same for all clocks
            clocks[i + 1].UpdateClockArmsConfig(firstArmConfig, secondArmConfig);
        }
    }

    public static void SetDefaultConfig(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        for (var i = 0; i < 24; i++)
        {
            clocks[i + 1].UpdateClockArmsConfig(selectFirstArmConfig(i), selectSecondArmConfig(i));
        }
    }

    public static void SetReverseConfig(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
    {
        var j = 0;
        for (var i = 23; i >= 0; i--)
        {
            clocks[i + 1].UpdateClockArmsConfig(selectFirstArmConfig(j), selectSecondArmConfig(j));
            j++;
        }
    }

    public static void SetByRowConfig(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
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

    public static void SetCenterOutConfig(Dictionary<int, Clock> clocks, Func<Clock, int, Components.AnimationConfig> selectFirstArmConfig, Func<Clock, int, Components.AnimationConfig> selectSecondArmConfig)
    {
        var groups = new[]
        {
            [11, 14],
            [7, 8, 9, 16, 17, 18, 10, 13, 12, 15],
            [4, 5, 6, 19, 20, 21],
            new[] { 1, 2, 3, 22, 23, 24 }
        };

        for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            foreach (var clockIndex in groups[groupIndex])
            {
                clocks[clockIndex].UpdateClockArmsConfig(
                    selectFirstArmConfig(clocks[clockIndex], groupIndex),
                    selectSecondArmConfig(clocks[clockIndex], groupIndex));
            }
        }
    }

    public static void SetSpiralConfig(Dictionary<int, Clock> clocks, Func<int, Components.AnimationConfig> selectFirstArmConfig, Func<int, Components.AnimationConfig> selectSecondArmConfig)
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
}

public enum AnimationConfigType
{
    Default,
    Reverse,
    ByRow,
    CenterOut,
    Spiral
}