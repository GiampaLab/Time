using Time.AnimationConfig;

public class Animation
{
    private bool? animationSetupCompleted = false;
    private bool animationSettingsCompleted = false;
    public delegate bool? CallbackEventHandler(AnimationStatus status, double timeElapsed);
    public event CallbackEventHandler? OnStatusChanged;
    public event EventHandler? OnSettingsCompleted;
    public DelayAnimation DelayAnimation { get; set; } = new DelayAnimation();

    public void OnNextAnimationSettingsCompleted(object sender, EventArgs args)
    {
        animationSettingsCompleted = false;
        animationSetupCompleted = false;
        DelayAnimation.Started = false;
    }

    public void Start(string animationName, double timeElapsed)
    {
        if (animationSetupCompleted.HasValue && animationSetupCompleted.Value)
        {
            if (!animationSettingsCompleted)
            {
                Console.WriteLine($"Animation setup {animationName}");
                animationSettingsCompleted = true;
                OnSettingsCompleted?.Invoke(this, EventArgs.Empty);
                OnStatusChanged?.Invoke(AnimationStatus.PerformPreAnimationSettings, timeElapsed);
            }
            if (DelayAnimation.TimeIsUp(timeElapsed))
            {
                Console.WriteLine($"Animation Start {animationName}");
                OnStatusChanged?.Invoke(AnimationStatus.StartAnimation, timeElapsed);
            }
        }
        else
        {
            Console.WriteLine($"Animation Init {animationName}");
            animationSetupCompleted = OnStatusChanged?.Invoke(AnimationStatus.StartInitAnimation, timeElapsed);
        }
    }
}

public enum AnimationStatus
{
    StartInitAnimation,
    StartAnimation,
    PerformPreAnimationSettings
}