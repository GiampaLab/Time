using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;

public class PatternAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks,
    Func<AnimationPatternType> SetPatternAnimationStatus) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<Components.AnimationConfig> animationInfo = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();
    private DotNetObjectReference<IAnimationManager>? myDotNetObjectReference;
    public AnimationPatternType AnimationPatternType { get; private set; }
    public bool IsFinished { get; private set; }

    public async void Start()
    {
        IsFinished = false;

        AnimationPatternType = SetPatternAnimationStatus();

        var animationConfigsArray = animationInfo.Select(AnimationUtils.MapAnimationConfig);

        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArm", new[] { myDotNetObjectReference },
            animationConfigsArray.ToArray());
    }

    [JSInvokable]
    public void AnimationFinished()
    {
        IsFinished = true;
    }

    public void Stop()
    {
    }

    public AnimationType GetAnimationType()
    {
        return AnimationType.Pattern;
    }

    public void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference)
    {
        myDotNetObjectReference = dotNetObjectReference;
    }

    public void Dispose()
    {
    }
}