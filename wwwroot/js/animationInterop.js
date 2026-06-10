if (typeof screen !== 'undefined' && screen.orientation && screen.orientation.lock) {
  screen.orientation.lock('landscape').catch(function () {});
}

function enforceLandscape() {
  var el = document.querySelector('.clocks-wrapper');
  if (!el) return;
  var W = window.innerWidth;
  var H = window.innerHeight;
  if (H <= W) {
    el.style.cssText = '';
    return;
  }
  // Portrait: rotate content to fill landscape dimensions
  el.style.width = H + 'px';
  el.style.height = W + 'px';
  el.style.position = 'fixed';
  el.style.top = ((H - W) / 2) + 'px';
  el.style.left = ((W - H) / 2) + 'px';
  el.style.transform = 'rotate(90deg)';
  el.style.transformOrigin = (H / 2) + 'px ' + (W / 2) + 'px';
}

window.addEventListener('load', function () {
  // .clocks-wrapper is rendered by Blazor after the framework boots,
  // so retry until it appears before applying the rotation.
  (function retry() {
    if (document.querySelector('.clocks-wrapper')) {
      enforceLandscape();
    } else {
      setTimeout(retry, 100);
    }
  })();
});
window.addEventListener('resize', enforceLandscape);

// Keep track of the previous animation to know the start point of the next animation
// If it's a chained continuous animation I need to calculate the end current angle and start from there
var previousAnimationConfigs = [];
var animations = [];
var animationConfigs = [];

window.animationLoop = {
  initShadows: function (animationConfigs) {
    this.animationConfigs = animationConfigs;
    this.animationConfigs.forEach(function (item) {
      if (item != null) {
        updateShadow(item);
      }
    });
    // Update the shadow every second (or more frequently)
    // TODO use WAAPI instead of setInterval
    // setInterval(() => {
    // setInterval(updateShadow(this.animationConfigs), 1000);
  },
  animateClockArm: function (dotNetObjectReference, animationConfigs) {
    animations = [];
    if (previousAnimationConfigs.length == 0) {
      previousAnimationConfigs = Array.from({ length: animationConfigs.length }, () => {
        return createDefaultAnimationConfig();
      });
    }
    animationConfigs.forEach(function (item, index) {
      setTimeout(() => {
        if (previousAnimationConfigs.length > 0) {
          //previousAnimationConfigs.forEach((previousAnimationConfig, index) => {
          if (previousAnimationConfigs[index].state == null) {
            // If the animation is continuous, we need to get the current rotation angle of the element
            var currentAngle = getCurrentRotationAngle(previousAnimationConfigs[index].elementReference);
            previousAnimationConfigs[index].state = currentAngle;
            // To chain the animation with the previous one we need to set the same direction
            animationConfigs[index].direction = previousAnimationConfigs[index].direction;
          }
          //});
        }
        const previousRotationDegrees = previousAnimationConfigs[index].state;
        const keyframes = generateKeyframesWithClockDirection(previousRotationDegrees, item.state, item.direction, 3);
        let animation = item.elementReference.animate(keyframes, {
          // timing options
          duration: item.duration,
          iterations: 1,
          fill: "both",
          easing: item.easing,
        });
        // The new animation (fill:both) now holds the arm, so release the stale
        // ones it overrides. The live angle was already read above, so the handoff
        // is unaffected.
        sweepStaleAnimations(item.elementReference, animation);
        animations.push(animation.finished);
        animation.finished.then(() => {
          previousAnimationConfigs[index] = item;
        });
        if (animations.length == animationConfigs.length) {
          Promise.all(animations).then(() => {
            if (dotNetObjectReference != null) {
              dotNetObjectReference.forEach((element) => {
                element.invokeMethodAsync("AnimationFinished");
              });
            }
          });
        }
      }, item.delay);
    });
  },
  animateClockArmInfinite: function (something, animationConfigs) {
    animations = [];
    if (previousAnimationConfigs.length == 0) {
      previousAnimationConfigs = Array.from({ length: animationConfigs.length }, () => {
        return createDefaultAnimationConfig();
      });
    }
    animationConfigs.forEach(function (item, index) {
      let targetAngle = item.direction === "Clockwise" ? 360 : -360;
      // Pause using setTimeout
      setTimeout(() => {
        const windup = item.elementReference.animate(
          [{ transform: `rotate(${item.state}deg)` }, { transform: `rotate(${item.state + targetAngle}deg)` }],
          {
            duration: item.duration * 1.6,
            iterations: 1,
            fill: "both",
            easing: item.easing,
          }
        );
        // Wind-up holds the arm now: release the previous phase's stale/infinite animations.
        sweepStaleAnimations(item.elementReference, windup);
        windup.finished
          .then(() => {
            const spin = item.elementReference.animate(
              [{ transform: `rotate(${item.state}deg)` }, { transform: `rotate(${item.state + targetAngle}deg)` }],
              {
                duration: item.duration,
                iterations: Infinity,
                fill: "both",
                easing: "linear",
              }
            );
            // Infinite spin takes over: retire the now-finished wind-up.
            sweepStaleAnimations(item.elementReference, spin);
          })
          .catch(() => {
            /* wind-up was cancelled before completing; nothing to hand off */
          });
        // We do not know the end state of the animation, so we set it to null
        // and we will calculate it when the animation is finished
        previousAnimationConfigs[index].state = null;
        previousAnimationConfigs[index].direction = item.direction;
      }, item.delay);
    });
  },
  // Shared "swing" primitive used by any pattern whose needles sway around a resting angle
  // (Pendulum, Wave, ...). Each needle eases out of its posed angle, then sways forever
  // between ±item.amplitude around it, easing in/out at each end like a pendulum or a stalk
  // in the wind. Amplitude is per-config so callers pick their own feel; the per-arm
  // item.delay staggers the launches so a shared period resolves into a travelling wave.
  animateClockArmSwing: function (something, animationConfigs) {
    animations = [];
    if (previousAnimationConfigs.length == 0) {
      previousAnimationConfigs = Array.from({ length: animationConfigs.length }, () => {
        return createDefaultAnimationConfig();
      });
    }
    animationConfigs.forEach(function (item, index) {
      const center = item.state;
      const to = center + item.amplitude;
      const from = center - item.amplitude;
      setTimeout(() => {
        // Ease out from the resting angle to one extreme (half a swing) so the motion
        // starts smoothly from exactly where it was posed — no snap to a keyframe edge...
        const windup = item.elementReference.animate(
          [{ transform: `rotate(${center}deg)` }, { transform: `rotate(${to}deg)` }],
          {
            duration: item.duration * 0.5,
            iterations: 1,
            fill: "both",
            easing: "ease-out",
          }
        );
        // Wind-up holds the arm now: release the previous phase's stale/infinite animations.
        sweepStaleAnimations(item.elementReference, windup);
        windup.finished
          .then(() => {
            // ...then swing forever between the two extremes, easing in and out at each
            // end like a real pendulum / a stalk swaying in the wind.
            const swing = item.elementReference.animate(
              [{ transform: `rotate(${to}deg)` }, { transform: `rotate(${from}deg)` }],
              {
                duration: item.duration,
                iterations: Infinity,
                direction: "alternate",
                fill: "both",
                easing: "ease-in-out",
              }
            );
            // Swing takes over: retire the now-finished wind-up.
            sweepStaleAnimations(item.elementReference, swing);
          })
          .catch(() => {
            /* wind-up was cancelled before completing; nothing to hand off */
          });
        // The swing never settles, so mark the end state unknown; the next finite
        // animation will read the live angle to chain from it without jumping.
        previousAnimationConfigs[index].state = null;
        previousAnimationConfigs[index].direction = item.direction;
      }, item.delay);
    });
  },
  // Bounded "breathe" primitive (Starburst): a needle oscillates ONE-sided between its posed
  // resting angle and `center + item.amplitude`, forever. Unlike the swing (which sweeps
  // symmetrically about center), the resting angle is an ENDPOINT of the oscillation, so an
  // `alternate` animation already starts exactly at `center` — no intro half-swing and no
  // snap. Two arms with opposite-sign amplitudes (+A / -A) bloom open into a V and close back
  // to the overlapped ray, making each clock breathe. The per-arm item.delay can stagger the
  // launches (0 = the whole field breathes in unison; >0 = a center-out ripple).
  animateClockArmBreathe: function (something, animationConfigs) {
    animations = [];
    if (previousAnimationConfigs.length == 0) {
      previousAnimationConfigs = Array.from({ length: animationConfigs.length }, () => {
        return createDefaultAnimationConfig();
      });
    }
    animationConfigs.forEach(function (item, index) {
      const center = item.state;
      const to = center + item.amplitude;
      setTimeout(() => {
        const breath = item.elementReference.animate(
          [{ transform: `rotate(${center}deg)` }, { transform: `rotate(${to}deg)` }],
          {
            duration: item.duration,
            iterations: Infinity,
            direction: "alternate",
            fill: "both",
            easing: "ease-in-out",
          }
        );
        // The breath takes over the arm: release the previous phase's stale/infinite animations.
        sweepStaleAnimations(item.elementReference, breath);
        // The breath never settles, so mark the end state unknown; the next finite
        // animation will read the live angle to chain from it without jumping.
        previousAnimationConfigs[index].state = null;
        previousAnimationConfigs[index].direction = item.direction;
      }, item.delay);
    });
  },
};

window.animationInterop = {
  requestFullscreen: function () {
    if (document.documentElement.requestFullscreen) {
      document.documentElement.requestFullscreen();
    } else if (document.documentElement.mozRequestFullScreen) {
      // Firefox
      document.documentElement.mozRequestFullScreen();
    } else if (document.documentElement.webkitRequestFullscreen) {
      // Chrome, Safari, and Opera
      document.documentElement.webkitRequestFullscreen();
    } else if (document.documentElement.msRequestFullscreen) {
      // IE/Edge
      document.documentElement.msRequestFullscreen();
    }
  },
  // Persist the chosen clock skin ("classic" / "glass" / "aurora") so it survives
  // reloads. Wrapped in try/catch because localStorage can throw in private-mode /
  // sandboxed WebViews (Android, the Windows screensaver host) — we just fall back
  // to the default.
  getThemePref: function () {
    try {
      return localStorage.getItem("clockTheme");
    } catch (e) {
      return null;
    }
  },
  setThemePref: function (value) {
    try {
      localStorage.setItem("clockTheme", value);
    } catch (e) {
      /* storage unavailable — preference simply won't persist */
    }
  },
  // Let the user cycle through the clock skins from the keyboard (press "g"). The
  // key press is routed back to the Blazor component so C# stays the source of truth.
  registerThemeKeyToggle: function (dotNetRef) {
    document.addEventListener("keydown", function (e) {
      if (e.key === "g" || e.key === "G") {
        dotNetRef.invokeMethodAsync("CycleSkinFromJs");
      }
    });
  },
};

// Reusable function to create a default animation configuration
function createDefaultAnimationConfig() {
  return { state: 0, elementReference: null, direction: "Clockwise", duration: 0, delay: 0, easing: "linear" };
}

// Release an arm's stale animations once a new one has taken over. We only cancel
// animations that are already invisible and safe to drop:
//   - finished (filling) ones — overridden by the new keeper; their `finished`
//     promise has already resolved, so cancelling rejects nothing that's awaited.
//   - infinite ones (iterations === Infinity) — the previous phase's spin/swing,
//     overridden in composite order; their `finished` promise is never observed.
// A still-running finite animation (a wind-up or an in-progress pose) is left
// alone, so we never reject a promise that drives AnimationFinished or the
// wind-up -> infinite handoff; it finishes on its own and is reclaimed next sweep.
// `keep` is the just-created animation that now holds the arm and must survive.
function sweepStaleAnimations(element, keep) {
  for (const anim of element.getAnimations()) {
    if (anim === keep) continue;
    const timing = anim.effect && anim.effect.getTiming ? anim.effect.getTiming() : null;
    const isInfinite = timing && timing.iterations === Infinity;
    if (anim.playState === "finished" || isInfinite) {
      try {
        anim.cancel();
      } catch (e) {
        /* already detached from the element */
      }
    }
  }
}

function generateKeyframesWithClockDirection(currentAngle, targetAngle, direction, numKeyframes) {
  const keyframes = [];
  let difference = 0;
  let calculatedCurrentAngle = 0;
  let calculatedTargetAngle = 0;
  if (direction === "Clockwise") {
    calculatedCurrentAngle = currentAngle % 360;
    difference = targetAngle - calculatedCurrentAngle;
    if (difference < 0) {
      // force clockwise rotation through the 360 degrees mark
      calculatedTargetAngle = targetAngle + 360;
    } else {
      calculatedTargetAngle = targetAngle;
    }
  } else {
    let anticlockwiseTargetAngle = (-360 + targetAngle) % 360;
    calculatedCurrentAngle = currentAngle <= 0 ? currentAngle % 360 : (-360 + currentAngle) % 360;
    difference = Math.abs(anticlockwiseTargetAngle) - Math.abs(calculatedCurrentAngle);
    // not crossing the 360 degrees mark so we can just add the target angle to the current angle -> rotation should always be anticlockwise
    if (difference >= 0) {
      calculatedTargetAngle = anticlockwiseTargetAngle;
      // crossing the 360 degrees mark so we need to force the rotation to be anticlockwise
    } else {
      calculatedTargetAngle = anticlockwiseTargetAngle - 360;
    }
  }

  difference = calculatedTargetAngle - calculatedCurrentAngle;
  let increment = difference / (numKeyframes + 1);
  let nextAngle = calculatedCurrentAngle;
  keyframes.push({ transform: `rotate(${calculatedCurrentAngle}deg)` });

  for (let i = 0; i < numKeyframes; i++) {
    nextAngle += increment;
    keyframes.push({ transform: `rotate(${nextAngle}deg)` });
  }

  keyframes.push({ transform: `rotate(${calculatedTargetAngle}deg)` });
  return keyframes;
}

function getCurrentRotationAngle(element) {
  const style = window.getComputedStyle(element);
  const transform = style.getPropertyValue("transform");

  if (!transform || transform === "none") {
    return 0; // No transform applied
  }

  if (transform.startsWith("matrix(")) {
    // Extract matrix values
    const matrixValues = transform
      .match(/matrix\(([^)]+)\)/)[1]
      .split(",")
      .map(Number);
    const a = matrixValues[0];
    const b = matrixValues[1];

    // Calculate rotation angle in degrees
    const angle = Math.round(Math.atan2(b, a) * (180 / Math.PI));
    return angle;
  } else if (transform.startsWith("rotate(")) {
    // Extract rotate value
    const angleString = transform.match(/rotate\(([^)]+)\)/)[1];
    const angle = parseFloat(angleString);
    return angle;
  } else {
    return 0; // Unknown transform type
  }
}

function updateShadow(element) {
  const now = new Date();
  const hours = now.getHours() % 12; // 0-11
  const minutes = now.getMinutes();

  // Combine hours and minutes to get a more precise angle (optional)
  const totalMinutes = hours * 60 + minutes;
  const angle = (totalMinutes / 720) * 2 * Math.PI; // Map time to a 0-2*PI angle

  // Calculate shadow offset based on the angle (adjust the multiplier for distance)
  const shadowOffsetX = Math.sin(angle) * 5;
  const shadowOffsetY = -Math.cos(angle) * 5;

  const shadowBlur = 8;
  const shadowColor = "rgba(0, 0, 0, 0.3)";
  const shadowStyle = `inset ${shadowOffsetX}px ${shadowOffsetY}px ${shadowBlur}px ${shadowColor}`;

  element.style.boxShadow = shadowStyle;
  // element.animate([{ boxShadow: shadowStyle }], {
  //   duration: 1000,
  //   easing: "linear",
  // });
}
