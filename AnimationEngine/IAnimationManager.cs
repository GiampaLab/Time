using Microsoft.JSInterop;

namespace Time.AnimationEngine;

public interface IAnimationManager
{
    void Start();
    void Stop();
    [JSInvokable]
    void AnimationFinished();
    AnimationType GetAnimationType();
    void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference);
}
