using Microsoft.JSInterop;
using Time.AnimationConfig;
using Time.Components;

namespace Time.AnimationEngine;
public class InfiniteAnimationManager(IJSRuntime jSRuntime, Dictionary<int, Clock> clocks, Action<Dictionary<int, Clock>> SetInfiniteAnimationConfig) : IAnimationManager
{
    private readonly IJSRuntime jSRuntime = jSRuntime;
    private readonly Dictionary<int, Clock> clocks = clocks;
    private readonly IList<Components.AnimationConfig> animationConfigs = clocks.Values.SelectMany(x =>
            new[] { x.FirstArm.Config, x.SecondArm.Config }).ToArray();

    public bool IsFinished { get; private set; } = false;

    public async void Start()
    {
        SetInfiniteAnimationConfig(clocks);

        var args = animationConfigs.Select(AnimationUtils.MapAnimationConfig);

        await jSRuntime.InvokeVoidAsync("animationLoop.animateClockArmInfinite", null, args);
    }

    [JSInvokable]
    public void AnimationFinished()
    {
    }

    public AnimationType GetAnimationType()
    {
        return AnimationType.Infinite;
    }

    public void SetDotNetObjectReference(DotNetObjectReference<IAnimationManager> dotNetObjectReference)
    {
    }

    public void Stop()
    {
    }

    public void Dispose()
    {
    }
}