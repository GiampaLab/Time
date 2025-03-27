namespace Time.AnimationEngine;

public class ChainedAnimationsManager(IList<IAnimationManager> animationManagers, IList<int> animationDurations) : IAnimationManager
{
    private readonly IList<IAnimationManager> animationManagers = animationManagers;
    private readonly IList<int> animationDurations = animationDurations;
    private bool stop = false;

    private bool animationLoopStarted = false;
    public async void Start()
    {
        while (!stop)
        {
            foreach (var animationManager in animationManagers)
            {
                // if (!animationLoopStarted)
                animationManager.Start();
                // else
                //     animationManager.Continue();
                animationLoopStarted = true;
                await Task.Delay(animationDurations[animationManagers.IndexOf(animationManager)]);
                animationManager.Stop();
            }
        }
    }

    public void Stop()
    {
        stop = true;
        animationLoopStarted = false;
    }

    public void AnimationFinished()
    {
    }

    public void Continue()
    {
        throw new NotImplementedException();
    }
}