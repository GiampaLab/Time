// Keep track of the previous animation to know the start point of the next animation
// If it's a chained continuous animation I need to calculate the end current angle and start from there
var previousAnimationConfigs = [];
var animations = [];
var currentAnimationConfigs = [];

window.animationLoop = {
  updateState: function (hourRef, minuteRef, hourState, minuteState) {
    hourRef.style.transform = "rotate(" + hourState + "deg)";
    minuteRef.style.transform = "rotate(" + minuteState + "deg)";
  },
  animateClockArm: function (dotNetObjectReference, animationConfigs, chainAnimations) {
    if (previousAnimationConfigs.length == 0) {
      if (animations.length > 0) {
        animations.forEach((animation, index) => {
          if (index < currentAnimationConfigs.length) {
            var currentAngle = getCurrentRotationAngle(currentAnimationConfigs[index].elementReference);
            previousAnimationConfigs.push({ state: currentAngle });
          }
        });
        animations = [];
      } else {
        previousAnimationConfigs = Array.from({ length: animationConfigs.length }, () => {
          return { state: 0 };
        });
      }
    }
    currentAnimationConfigs = animationConfigs;
    currentAnimationConfigs.forEach(function (item, index) {
      const previousRotationDegrees = previousAnimationConfigs[index].state;
      const keyframes = generateKeyframesWithClockDirection(previousRotationDegrees, item.state, item.direction, 3);
      let animation = item.elementReference.animate(keyframes, {
        // timing options
        duration: item.duration,
        iterations: 1,
        fill: "both",
        easing: item.easing,
      });
      animation.finished.then(() => {
        dotNetObjectReference.invokeMethodAsync("AnimationFinished");
        if (chainAnimations == true) {
          var lastKeyframeAngle = getAngleFromKeyframe(keyframes[keyframes.length - 1]);
          // Second Animation: Continuous Rotation
          let targetAngle = item.direction === "Clockwise" ? 360 : -360;
          // Pause using setTimeout
          setTimeout(() => {
            animations.push(
              item.elementReference.animate([{ transform: `rotate(${lastKeyframeAngle}deg)` }, { transform: `rotate(${lastKeyframeAngle + targetAngle}deg)` }], {
                duration: item.duration,
                iterations: Infinity,
                fill: "both",
                easing: item.easing,
              })
            );
          }, item.delay);
        }
      });
    });
    if (chainAnimations == false) {
      previousAnimationConfigs = animationConfigs;
    } else {
      // I need to reset the previousAnimationConfigs to avoid the continuous rotation to be chained to the previous animation
      // Previous animation will be calculated from the current state of the clock arms at the top of this function
      previousAnimationConfigs = [];
    }
  },
  pauseClockArmAnimation: function () {
    // animations.forEach((animation, index) => {
    //   //animation.pause();
    //   var currentAngle = getCurrentRotationAngle(currentAnimationConfigs[index].elementReference);
    //   previousAnimationConfigs[index].state = currentAngle;
    // });
  },
};

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

function getAngleFromKeyframe(keyframe) {
  const transform = keyframe.transform;

  if (transform && transform.startsWith("rotate(") && transform.endsWith("deg)")) {
    const angleString = transform.slice(7, -4); // Extract the angle string
    const angle = parseFloat(angleString);
    return angle;
  }

  return null; // Return null if the keyframe is invalid
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
