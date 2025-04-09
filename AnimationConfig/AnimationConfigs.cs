using Time.Components;

namespace Time.AnimationConfig;
public class AnimationConfigs
{
    public static int StaggeredAnimation(int index, int milliseconds, int staggeredMilliseconds)
    {
        return milliseconds + ((index % 2) == 1 ? (index - 1) * staggeredMilliseconds : index * staggeredMilliseconds);
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
        var spiralOrder = new[]
        {
            14, 15, 12, 11, 10, 13, 16, 17, 18, 9, 8, 7, 19, 20, 21, 6, 5, 4, 22, 23, 24, 3, 2, 1
        };

        for (var i = 0; i < spiralOrder.Length; i++)
        {
            clocks[spiralOrder[i]].UpdateClockArmsConfig(selectFirstArmConfig(i), selectSecondArmConfig(i));
        }
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