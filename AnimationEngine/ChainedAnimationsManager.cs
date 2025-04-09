using Microsoft.JSInterop;

namespace Time.AnimationEngine;

public partial class ChainedAnimationsManager : IAnimationManager
{
    private readonly IList<(IAnimationManager, int)>? animationManagers;
    private readonly DotNetObjectReference<IAnimationManager> dotNetObjectReference;
    private readonly Func<IAnimationManager?, (IAnimationManager animationManager, int duration)>? nextAnimationManager;
    private (IAnimationManager animationManager, int duration) animationInfo;
    private bool animationManagerTimeIsUp = false;
    private int animationManagerIndex;
    public bool IsFinished { get; private set; }

    public ChainedAnimationsManager(IList<(IAnimationManager animationManager, int duration)> animationManagers)
    {
        this.animationManagers = animationManagers;
        dotNetObjectReference = CreateDotNetObjectReference();
        foreach (var animationManager in animationManagers)
        {
            animationManager.animationManager.SetDotNetObjectReference(dotNetObjectReference);
        }
    }
    public ChainedAnimationsManager(Func<IAnimationManager?, (IAnimationManager animationManager, int duration)> NextAnimationManager)
    {
        nextAnimationManager = NextAnimationManager;
        dotNetObjectReference = CreateDotNetObjectReference();
    }
    private DotNetObjectReference<IAnimationManager> CreateDotNetObjectReference()
    {
        return DotNetObjectReference.Create<IAnimationManager>(this);
    }
    public async void Start()
    {
        IsFinished = false;
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
        IsFinished = true;
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

    private (IAnimationManager, int) GetNextAnimationManager((IAnimationManager animationManager, int duration)? previousAnimationManager)
    {
        if (animationManagers == null)
        {
            if (nextAnimationManager == null)
            {
                throw new InvalidOperationException("nextAnimationManager cannot be null.");
            }
            var currentAnimationManager = nextAnimationManager(previousAnimationManager?.animationManager);
            currentAnimationManager.animationManager.SetDotNetObjectReference(dotNetObjectReference);
            return currentAnimationManager;
        }
        return animationManagers[animationManagerIndex++ % animationManagers.Count];
    }

    private async Task InternalStart((IAnimationManager, int)? previousAnimationManager)
    {
        animationInfo = GetNextAnimationManager(previousAnimationManager);
        animationInfo.animationManager.Start();
        await Task.Delay(animationInfo.duration);
        animationManagerTimeIsUp = true;
        if (animationInfo.animationManager.GetAnimationType() == AnimationType.Infinite || animationInfo.animationManager.IsFinished)
        {
            //needs to be triggered manually as the animation is either infinite or id doesn't continue triggering another animation(Pattern)
            // The Time and infinite animations never finish
            // The time animation will stop the next time the time changes
            AnimationFinished();
        }
    }

    public void Dispose()
    {
        dotNetObjectReference?.Dispose();
        // foreach (var animationManager in animationManagers)
        // {
        //     animationManager.Item1.Dispose();
        // }
    }
}