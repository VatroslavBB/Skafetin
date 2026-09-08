
window.skafetinQrScanner = {
    _stream: null,
    _timer: null,
    _canvas: null,

    unsupportedReason: function () {
        if (!window.isSecureContext)
            return "Kamera radi samo preko HTTPS-a ili na localhostu.";

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia)
            return "Preglednik ne podržava pristup kameri.";

        if (typeof jsQR !== "function")
            return "Čitač QR kodova nije učitan.";

        return null;
    },

    start: async function (videoElementId, dotNetRef) {
        const video = document.getElementById(videoElementId);
        if (!video)
            return "Element za prikaz kamere nije pronađen.";

        try {
            this._stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: "environment" }
            });
        } catch {
            return "Pristup kameri nije odobren.";
        }

        video.srcObject = this._stream;
        await video.play();

        this._canvas = document.createElement("canvas");
        const context = this._canvas.getContext("2d", { willReadFrequently: true });
        const scanner = this;

        this._timer = setInterval(async () => {
            if (video.readyState !== video.HAVE_ENOUGH_DATA)
                return;

            // Veci kadar ne pomaze citanju, a osjetno usporava dekodiranje.
            const width = Math.min(video.videoWidth, 640);
            const height = Math.round(video.videoHeight * (width / video.videoWidth));

            if (width === 0 || height === 0)
                return;

            scanner._canvas.width = width;
            scanner._canvas.height = height;
            context.drawImage(video, 0, 0, width, height);

            const image = context.getImageData(0, 0, width, height);
            const code = jsQR(image.data, width, height, { inversionAttempts: "dontInvert" });

            if (!code || !code.data)
                return;

            scanner.stop();
            await dotNetRef.invokeMethodAsync("OnCodeDetected", code.data);
        }, 200);

        return null;
    },

    stop: function () {
        if (this._timer) {
            clearInterval(this._timer);
            this._timer = null;
        }

        if (this._stream) {
            this._stream.getTracks().forEach(track => track.stop());
            this._stream = null;
        }

        this._canvas = null;
    }
};
