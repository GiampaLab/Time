// Keep track of the previous animation to know the start point of the next animation
// If it's a chained continuous animation I need to calculate the end current angle and start from there
var previousAnimationConfigs = [];
var animations = [];

window.animationLoop = {
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
        item.elementReference
          .animate([{ transform: `rotate(${item.state}deg)` }, { transform: `rotate(${item.state + targetAngle}deg)` }], {
            duration: item.duration * 1.6,
            iterations: 1,
            fill: "both",
            easing: item.easing,
          })
          .finished.then(() => {
            item.elementReference.animate([{ transform: `rotate(${item.state}deg)` }, { transform: `rotate(${item.state + targetAngle}deg)` }], {
              duration: item.duration,
              iterations: Infinity,
              fill: "both",
              easing: "linear",
            });
          });
        // We do not know the end state of the animation, so we set it to null
        // and we will calculate it when the animation is finished
        previousAnimationConfigs[index].state = null;
        previousAnimationConfigs[index].direction = item.direction;
      }, item.delay);
    });
  },
};

// Reusable function to create a default animation configuration
function createDefaultAnimationConfig() {
  return { state: 0, elementReference: null, direction: "Clockwise", duration: 0, delay: 0, easing: "linear" };
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
