window.animationLoop = {
    requestAnimationFrame: function (dotNetHelper) {
       // console.log("Requesting animation frame...");
        let frame = function (timeElapsed) {
            dotNetHelper.invokeMethodAsync('UpdateFrame', timeElapsed)
                .then(() => requestAnimationFrame(frame));
        };

        //console.log("Starting animation loop...");
        requestAnimationFrame(frame);
    },
    updateState: function(hourRef, minuteRef, hourState, minuteState){
        hourRef.style.transform = "rotate("+hourState+"deg)"
        minuteRef.style.transform = "rotate("+minuteState+"deg)"
    }
};