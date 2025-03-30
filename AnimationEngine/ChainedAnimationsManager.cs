using Microsoft.JSInterop;

namespace Time.AnimationEngine;

public partial class ChainedAnimationsManager : IAnimationManager
{
    private readonly IList<IAnimationManager> animationManagers;
    private readonly IList<int> animationDurations;
    private IAnimationManager? animationManager;
    private bool animationManagerTimeIsUp = false;

    private int animationManagerIndex;

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
        await InternalStart(null);
    }

    [JSInvokable]
    public async void AnimationFinished()
    {
        Console.WriteLine("Animation finished, Animation manager: " + animationManager?.GetAnimationType());
        if (!animationManagerTimeIsUp)
        {
            // Delegate the call to the animation manager
            animationManager?.AnimationFinished();
            return;
        }
        animationManagerTimeIsUp = false;
        animationManager?.Stop();
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

    private IAnimationManager GetNextAnimationManager()
    {
        return animationManagers[animationManagerIndex++ % animationManagers.Count];
    }

    private async Task InternalStart(IAnimationManager? previousAnimationManager)
    {
        animationManager = GetNextAnimationManager();
        animationManager.Start();
        await Task.Delay(animationDurations[animationManagers.IndexOf(animationManager)]);
        animationManagerTimeIsUp = true;
        if (animationManager.GetAnimationType() == AnimationType.Infinite || animationManager.GetAnimationType() == AnimationType.Pattern)
        {
            //needs to be triggered manually as the animation is infinite
            AnimationFinished();
        }
    }
}