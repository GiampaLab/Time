namespace Time.Utils;
public class DelayAnimation
{
    private double delayStartTimeMillisec = 0;
    public bool Started { get; set; } = false;
    public int DelayMillisec { get; set; } = 0;
    public bool TimeIsUp(double timeElapsed)
    {
        if (!Started)
        {
            delayStartTimeMillisec = timeElapsed;
            Started = true;
        }
        return timeElapsed - delayStartTimeMillisec >= DelayMillisec;
    }
}