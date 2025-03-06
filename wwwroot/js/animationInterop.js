window.animationLoop = {
  updateState: function (hourRef, minuteRef, hourState, minuteState) {
    hourRef.style.transform = "rotate(" + hourState + "deg)";
    minuteRef.style.transform = "rotate(" + minuteState + "deg)";
  },
  animateClockArm: function (animationConfigs) {
    animationConfigs.forEach(function (item, index) {
      //item.elementReference.style.transform = "rotate(" + item.state + "deg)";
      item.elementReference.animate(
        [
          // keyframes
          { transform: "rotate(" + item.state + "deg)" },
        ],
        {
          // timing options
          duration: 3000,
          iterations: 1,
          fill: "forwards",
          easing: "ease-in-out",
        }
      );
    });
  },
};
