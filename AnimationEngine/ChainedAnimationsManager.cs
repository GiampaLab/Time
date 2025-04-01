using Microsoft.JSInterop;

namespace Time.AnimationEngine;

public partial class ChainedAnimationsManager : IAnimationManager
{
    private readonly IList<(IAnimationManager, int)> animationManagers;
    private (IAnimationManager animationManager, int duration) animationInfo;
    private bool animationManagerTimeIsUp = false;
    private int animationManagerIndex;

    public ChainedAnimationsManager(IList<(IAnimationManager animationManager, int duration)> animationManagers)
    {
        this.animationManagers = animationManagers;
        var myDotNetObjectReference = DotNetObjectReference.Create<IAnimationManager>(this);
        foreach (var animationManager in animationManagers)
        {
            animationManager.animationManager.SetDotNetObjectReference(myDotNetObjectReference);
        }
    }

    public async void Start()
    {
        await InternalStart(null);
    }

    [JSInvokable]
    public async void AnimationFinished()
    {
        Console.WriteLine("Animation finished, Animation manager: " + animationInfo.animationManager.GetAnimationType());
        if (!animationManagerTimeIsUp)
        {
            // Delegate the call to the animation manager
            animationInfo.animationManager.AnimationFinished();
            return;
        }
        animationManagerTimeIsUp = false;
        animationInfo.animationManager.Stop();
        await InternalStart(animationInfo);
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
        animationInfo = GetNextAnimationManager();
        animationInfo.animationManager.Start();
        await Task.Delay(animationInfo.duration);
        animationManagerTimeIsUp = true;
        if (animationInfo.animationManager.GetAnimationType() == AnimationType.Infinite || animationInfo.animationManager.GetAnimationType() == AnimationType.Pattern)
        {
            //needs to be triggered manually as the animation is either infinite or id doesn't continue triggering another animation(Pattern)
            AnimationFinished();
        }
    }
}