var previousAnimationConfigs = [];

window.animationLoop = {
  updateState: function (hourRef, minuteRef, hourState, minuteState) {
    hourRef.style.transform = "rotate(" + hourState + "deg)";
    minuteRef.style.transform = "rotate(" + minuteState + "deg)";
  },
  animateClockArm: function (animationConfigs, chainAnimations) {
    if (previousAnimationConfigs.length == 0) {
      previousAnimationConfigs = Array.from(
        { length: animationConfigs.length },
        () => {
          return { elementReference: null, state: 0, easing: "linear" };
        }
      );
    }
    animationConfigs.forEach(function (item, index) {
      const previousRotationDegrees = previousAnimationConfigs[index].state;
      const keyframes = generateKeyframesWithClockDirection(
        previousRotationDegrees,
        item.state,
        item.direction,
        3
      );
      let animation = null;
      animation = item.elementReference.animate(keyframes, {
        // timing options
        duration: item.duration,
        iterations: 1,
        fill: "forwards",
        easing: item.easing,
      });
      animation.finished.then(() => {
        if (chainAnimations == true) {
          var lastKeyframeAngle = getAngleFromKeyframe(
            keyframes[keyframes.length - 1]
          );
          // Second Animation: Continuous Rotation
          let targetAngle = item.direction === "Clockwise" ? 360 : -360;
          // Pause using setTimeout
          setTimeout(() => {
            item.elementReference.animate(
              [
                { transform: `rotate(${lastKeyframeAngle}deg)` },
                { transform: `rotate(${lastKeyframeAngle + targetAngle}deg)` },
              ],
              {
                duration: item.duration,
                iterations: Infinity,
                easing: "linear",
              }
            );
          }, item.delay);
        }
      });
    });
    previousAnimationConfigs = animationConfigs;
  },
};

function generateKeyframesWithClockDirection(
  currentAngle,
  targetAngle,
  direction,
  numKeyframes
) {
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
    calculatedCurrentAngle =
      currentAngle <= 0 ? currentAngle % 360 : (-360 + currentAngle) % 360;
    difference =
      Math.abs(anticlockwiseTargetAngle) - Math.abs(calculatedCurrentAngle);
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
function getAngleFromKeyframe(keyframe) {
  const transform = keyframe.transform;

  if (
    transform &&
    transform.startsWith("rotate(") &&
    transform.endsWith("deg)")
  ) {
    const angleString = transform.slice(7, -4); // Extract the angle string
    const angle = parseFloat(angleString);
    return angle;
  }

  return null; // Return null if the keyframe is invalid
}
