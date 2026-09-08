
window.skafetinQrScanner = {
    _stream: null,
    _timer: null,

    unsupportedReason: function () {
        if (!window.isSecureContext)
            return "Kamera radi samo preko HTTPS-a ili na localhostu.";

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia)
            return "Preglednik ne podržava pristup kameri.";

        if (!("BarcodeDetector" in window))
            return "Preglednik ne podržava čitanje QR kodova. Koristi Chrome ili Edge.";

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

        const detector = new BarcodeDetector({ formats: ["qr_code"] });
        const scanner = this;

        this._timer = setInterval(async () => {
            if (video.readyState !== video.HAVE_ENOUGH_DATA)
                return;

            let codes;
            try {
                codes = await detector.detect(video);
            } catch {
                return;
            }

            if (codes.length === 0)
                return;

            scanner.stop();
            await dotNetRef.invokeMethodAsync("OnCodeDetected", codes[0].rawValue);
        }, 250);

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
    }
};

