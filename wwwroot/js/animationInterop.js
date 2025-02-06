currentRotationH = 0;
currentRotationM = 0;
hourRef = null;
minuteRef = null;
lastFPS = 0;
window.animationLoop = {
  requestAnimationFrame: function (dotNetHelper, hourRef, minuteRef) {
    // console.log("Requesting animation frame...");
    let frame = function (timeElapsed) {
      // this.currentRotationH += 1;
      // this.currentRotationM -=1;
      // for(let i = 1; i <=24; i++){
      //     hourRef = document.getElementById(i);
      //     minuteRef = document.getElementById(i + 24);
      //     hourRef.style.transform = "rotate("+this.currentRotationH+"deg)";
      //     minuteRef.style.transform = "rotate("+this.currentRotationM+"deg)";
      // }
      //requestAnimationFrame(frame);
      //   let now = Date.now();
      //   if (now - lastFPS >= 1) {
      //     // console.log("FPS: " + (1000 / (now - lastFPS)));
      //     lastFPS = now;
      dotNetHelper
        .invokeMethodAsync("UpdateFrame", timeElapsed)
        .then(() => requestAnimationFrame(frame));
      //   } else {
      //     requestAnimationFrame(frame);
      //   }
    };

    //console.log("Starting animation loop...");
    requestAnimationFrame(frame);
  },
  updateState: function (hourRef, minuteRef, hourState, minuteState) {
    hourRef.style.transform = "rotate(" + hourState + "deg)";
    minuteRef.style.transform = "rotate(" + minuteState + "deg)";
  },
};
