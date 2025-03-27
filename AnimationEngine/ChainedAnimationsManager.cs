namespace Time.AnimationEngine;

public class ChainedAnimationsManager(IList<IAnimationManager> animationManagers, IList<int> animationDurations) : IAnimationManager
{
    private readonly IList<IAnimationManager> animationManagers = animationManagers;
    private readonly IList<int> animationDurations = animationDurations;
    private bool stop = false;
    public async void Start()
    {
        while (!stop)
        {
            foreach (var animationManager in animationManagers)
            {
                animationManager.Start();
                await Task.Delay(animationDurations[animationManagers.IndexOf(animationManager)]);
                animationManager.Stop();
            }
        }
    }

    public void Stop()
    {
        stop = true;
    }

    public void AnimationFinished()
    {
    }
}