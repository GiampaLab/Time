window.animationLoop = {
    requestAnimationFrame: function (dotNetHelper) {
       // console.log("Requesting animation frame...");
        let frame = function () {
            //console.log("Calling .NET method UpdateFrame");
            dotNetHelper.invokeMethodAsync('UpdateFrame')
                .then(() => requestAnimationFrame(frame));
        };

        //console.log("Starting animation loop...");
        requestAnimationFrame(frame);
    }
};