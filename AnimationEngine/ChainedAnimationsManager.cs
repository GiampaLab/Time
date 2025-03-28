using Microsoft.JSInterop;

namespace Time.AnimationEngine;

public partial class ChainedAnimationsManager : IAnimationManager
{
    private readonly IList<IAnimationManager> animationManagers;
    private readonly IList<int> animationDurations;
    private IAnimationManager? animationManager;
    private bool animationManagerTimeIsUp = true;

    public ChainedAnimationsManager(IList<IAnimationManager> animationManagers, IList<int> animationDurations)
    {
        this.animationManagers = animationManagers;
        this.animationDurations = animationDurations;
        var myDotNetObjectReference = DotNetObjectReference.Create<IAnimationManager>(this);
        foreach (var animationManager in animationManagers)
        {
            animationManager.SetDotNetObjectReference(myDotNetObjectReference);
        }
    }

    public async void Start()
    {
        animationManager = animationManagers[0];
        await InternalStart(animationManager);
    }

    [JSInvokable]
    public async void AnimationFinished()
    {
        Console.WriteLine("Animation finished, Animation manager: " + animationManager.GetAnimationType());
        if (!animationManagerTimeIsUp)
        {
            // Delegate the call to the animation manager
            animationManager.AnimationFinished();
            return;
        }
        animationManagerTimeIsUp = false;
        animationManager.Stop();
        animationManager = GetNextAnimationManager(animationManager);

        await InternalStart(animationManager);
    }

    public void Stop()
    {
    }

    public AnimationType GetAnimationType()
    {
        return AnimationType.Chained;
    }

    public void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference)
    {
    }

    private IAnimationManager GetNextAnimationManager(IAnimationManager animationManager)
    {
        var index = animationManagers.IndexOf(animationManager);
        if (index == -1)
        {
            throw new ArgumentException("Animation manager not found in the list");
        }
        index++;
        if (index >= animationManagers.Count)
        {
            index = 0;
        }
        return animationManagers[index];
    }

    private async Task InternalStart(IAnimationManager animationManager)
    {
        animationManager.Start();
        await Task.Delay(animationDurations[animationManagers.IndexOf(animationManager)]);
        animationManagerTimeIsUp = true;
        if (animationManager.GetAnimationType() == AnimationType.Pattern)
        {
            //needs to be triggered manually as the animation is infinite
            AnimationFinished();
        }
    }
}