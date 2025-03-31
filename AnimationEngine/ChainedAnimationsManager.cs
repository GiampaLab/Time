using Microsoft.JSInterop;

namespace Time.AnimationEngine;

public partial class ChainedAnimationsManager : IAnimationManager
{
    private readonly IList<(IAnimationManager, int)> animationManagers;
    private (IAnimationManager, int) animationManager;
    private bool animationManagerTimeIsUp = false;
    private int animationManagerIndex;

    public ChainedAnimationsManager(IList<(IAnimationManager, int)> animationManagers)
    {
        this.animationManagers = animationManagers;
        var myDotNetObjectReference = DotNetObjectReference.Create<IAnimationManager>(this);
        foreach (var animationManager in animationManagers)
        {
            animationManager.Item1.SetDotNetObjectReference(myDotNetObjectReference);
        }
    }

    public async void Start()
    {
        await InternalStart(null);
    }

    [JSInvokable]
    public async void AnimationFinished()
    {
        Console.WriteLine("Animation finished, Animation manager: " + animationManager.Item1.GetAnimationType());
        if (!animationManagerTimeIsUp)
        {
            // Delegate the call to the animation manager
            animationManager.Item1.AnimationFinished();
            return;
        }
        animationManagerTimeIsUp = false;
        animationManager.Item1.Stop();
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

    private (IAnimationManager, int) GetNextAnimationManager()
    {
        return animationManagers[animationManagerIndex++ % animationManagers.Count];
    }

    private async Task InternalStart((IAnimationManager, int)? previousAnimationManager)
    {
        animationManager = GetNextAnimationManager();
        animationManager.Item1.Start();
        await Task.Delay(animationManager.Item2);
        animationManagerTimeIsUp = true;
        if (animationManager.Item1.GetAnimationType() == AnimationType.Infinite || animationManager.Item1.GetAnimationType() == AnimationType.Pattern)
        {
            //needs to be triggered manually as the animation is infinite
            AnimationFinished();
        }
    }
}