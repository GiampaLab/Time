using Microsoft.JSInterop;

namespace Time.AnimationEngine;

public interface IAnimationManager
{
    void Start();
    void Stop();
    void AnimationFinished();
    AnimationType GetAnimationType();
    void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference);
}
