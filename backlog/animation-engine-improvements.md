# Animation Engine Improvements — Phased Backlog

**Goal:** make the clock animation as smooth as possible and make the pattern-sequence
phases (Time → Pattern → Infinite → Time) interlock as seamlessly as possible.

**Scope:** the *core animation logic* — the WAAPI/JS layer (`wwwroot/js/animationInterop.js`)
and the C# orchestration (`AnimationEngine/`: `AnimationOrchestrator`,
`ChainedAnimationsManager`, `PatternAnimationManager`, `InfiniteAnimationManager`,
`TimeAnimationManager`). Per-pattern motion config now lives in each
`AnimationEngine/Patterns/*Pattern.cs` (`BuildInfinite`).

**What's already good (keep):** animations use only `transform: rotate(...)` (compositor-
friendly, no layout/paint); the `state = null` → read-live-angle handoff is a clever way to
catch a spinning arm and steer it into the next shape; `fill: "both"` preserves continuity.
The issues below are about *scheduling* and *lifecycle*, not the property choice.

**How to use this backlog:** phases are **priority-ordered** (highest value / lowest risk
first; largest, riskiest rework last). Each phase is independently shippable and verifiable,
so they can be done one PR at a time and stopped at any point with value already banked.

| Phase | Theme | Primary goal | Risk | Why this order |
|-------|-------|--------------|------|----------------|
| 1 | Animation lifecycle cleanup | Sustained smoothness | Low | Biggest payoff for a long-running screensaver, isolated to JS |
| 2 | Settle-gated phase transitions | Seamless interlocking | Med | Removes the most likely visible seam |
| 3 | Frame-accurate staggering | Smooth cascades | Low–Med | Localized JS change, immediate visual polish |
| 4 | JS internals hardening | Smoothness + correctness | Med | De-risks the rest; removes latent glitch sources |
| 5 | Foundational scheduling rework | Both goals, fully | High | Largest change; do last once everything above de-risks it |

---

## Phase 1 — Animation lifecycle cleanup (stop accumulation)
**Priority: highest. Goal: sustained smoothness. Risk: low.**

**Problem.** Every phase starts ~2 `fill: "both"` animations per arm × 48 arms and **never
`cancel()`s the old ones**. Filling animations persist indefinitely, so over a screensaver
running for hours, thousands of finished animations pile up on each element → gradual
memory growth and compositor jank. This is the most likely cause of "it gets less smooth
the longer it runs."

**Change.** In `animationInterop.js`, once the next animation for an arm starts, bake the
resting transform and drop the old animation:
`anim.commitStyles()` then `anim.cancel()` (or keep a single live `Animation` reference per
arm and replace it). Verify with `element.getAnimations().length` staying flat over time.

**Files.** `wwwroot/js/animationInterop.js` (`animateClockArm`, `animateClockArmInfinite`,
`animateClockArmPendulum`).

**Verify.** Instrument `getAnimations().length` on a sample arm during a long run; it should
stabilize instead of climbing. No visual regression across a full Time→Pattern→Infinite cycle.

---

## Phase 2 — Settle-gated phase transitions
**Priority: high. Goal: seamless interlocking. Risk: medium.**

**Problem.** Phase boundaries use a hardcoded `await Task.Delay(13000 / 25000)` in
`ChainedAnimationsManager.InternalStart`, decoupled from when the motion actually finishes.
The Pattern phase's real settle time is `~5000ms + staggered delays (up to ~300·N)` but the
phase is a fixed `13000ms`. If the stagger ever overruns the phase, the Infinite phase
starts spinning from `item.state` (the *target* angle) while some arms haven't physically
arrived → a visible snap. This is the most likely seam.

**Change.** Gate the Pattern → Infinite transition on the `Promise.all(finished)` the JS
already computes (surfaced via the existing `AnimationFinished` callback), with a *minimum*
dwell time, instead of a blind `Task.Delay`. Also align the phase duration with the config
(Phase 6 generalizes this) so they can't drift apart.

**Files.** `AnimationEngine/ChainedAnimationsManager.cs`, `AnimationEngine/PatternAnimationManager.cs`,
`wwwroot/js/animationInterop.js` (completion signaling).

**Verify.** Force-select a pattern with a large stagger; confirm the spin only begins after
the last arm has reached its posed angle (no snap at the Pattern→Infinite boundary).

---

## Phase 3 — Frame-accurate staggering (drop `setTimeout`)
**Priority: medium-high. Goal: smooth cascades. Risk: low–medium.**

**Problem.** Intra-phase staggering uses `setTimeout(() => …, item.delay)`. `setTimeout` is
not frame-aligned and is subject to jitter/coalescing, so a 48-arm staggered sweep starts
each arm a few milliseconds off — exactly the unevenness the eye catches in a slow, elegant
cascade.

**Change.** Use WAAPI's native timing instead: pass `delay: item.delay` in the animation's
timing options (frame-accurate, compositor-driven) rather than wrapping the `animate()` call
in `setTimeout`. (Phase 6 takes this further with explicit `startTime` on the shared
timeline.)

**Files.** `wwwroot/js/animationInterop.js` (all three `animateClockArm*` functions).

**Verify.** Slow-motion capture of a staggered pattern (e.g. Line/Flow) shows perfectly even
arm-to-arm spacing; no dependence on tab/JS load.

---

## Phase 4 — JS internals hardening
**Priority: medium. Goal: smoothness + correctness. Risk: medium.**

Three related JS cleanups that remove latent glitch sources and a forced reflow:

1. **Per-element state instead of module globals.** `previousAnimationConfigs`, `animations`,
   and `animationConfigs` are module-scoped mutable arrays shared across all invocations.
   Sequential orchestration mostly protects them, but any phase overlap corrupts state. Move
   to per-invocation state or a per-element `Map`.

2. **Analytic angle, no DOM read-back.** `getCurrentRotationAngle` calls
   `getComputedStyle().transform` and decomposes the matrix for each arm — a synchronous
   layout flush ×48 at a transition. It also `Math.round`s to whole degrees (sub-degree
   snap) and `atan2` wraps multi-turn spins to one revolution. Track the end/last angle
   analytically (you control the keyframes; for an infinite spin derive it from
   `animation.currentTime`) so transitions never read back from the DOM.

3. **Simplify keyframes.** `generateKeyframesWithClockDirection(..., 3)` hand-builds evenly-
   spaced intermediate angles, which slightly fights the easing. Pass just `[{from},{to}]`
   and let the easing interpolate; keep only the directional / 360°-crossing logic (that part
   is genuinely needed).

**Files.** `wwwroot/js/animationInterop.js`.

**Verify.** No forced-reflow warnings in DevTools at transitions; Infinite→Time handoff is
smooth with no sub-degree snap; refactored keyframes visually identical.

---

## Phase 5 — Foundational scheduling rework (the keystone)
**Priority: do last. Goal: both goals, fully. Risk: high (largest change).**

**Problem.** Timing comes from two wall-clock approximations stitched together —
`setTimeout` (JS staggers) and `Task.Delay` (C# phase boundaries) — neither frame-accurate
nor aware of each other. There are also two different handoff mechanisms (finite animations
chain from `previousAnimationConfigs[i].state`; infinite ones set it to `null` for read-back),
and `async void` `Start`/`AnimationFinished` make sequencing looser than it looks.

**Change.** Schedule the whole choreography on the shared `document.timeline` using explicit
`animation.startTime` (relative to `document.timeline.currentTime`), so the browser runs
staggers *and* phase sequencing on one clock, frame-perfectly — phase N+1 begins at an exact
offset from phase N rather than via a `Task.Delay` race. Alongside it:
- **Unify the handoff contract** so every phase reports its arms' resting angle the same way
  (one analytic mechanism, no `state = null` special case).
- **Replace `async void`** `Start`/`AnimationFinished` with `async Task` (at least
  internally) for deterministic ordering and surfaced exceptions.

This subsumes Phases 2–3's scheduling concerns; it's listed last because it's the biggest,
riskiest rework and is far safer once Phases 1–4 have banked the cheaper wins and hardened
the internals.

**Files.** `wwwroot/js/animationInterop.js`, `AnimationEngine/ChainedAnimationsManager.cs`,
all `AnimationEngine/*Manager.cs`, and the `AnimationEngine/Patterns/*Pattern.cs`
`BuildInfinite` configs (delay → startTime offsets).

**Verify.** A full multi-cycle run is glitch-free at every phase boundary; no `setTimeout`/
`Task.Delay` remain in the animation hot path; timings are derived, not hand-tuned magic
numbers.

---

### Notes
- Phases 1–4 are intentionally incremental and low-blast-radius; each can ship as its own PR.
- Phase 5 should be planned in its own right (likely its own multi-step plan) before starting.
- This layer was flagged as failure-prone; prefer small, individually verifiable steps and
  visual confirmation (run the app, watch a full cycle) over big-bang changes.
