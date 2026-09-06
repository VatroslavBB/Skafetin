

window.skafetinFiles = {
    open: function (base64, contentType, fileName) {
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);

        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }

        const url = URL.createObjectURL(new Blob([bytes], { type: contentType }));
        const opened = window.open(url, "_blank");
        if (!opened) {
            const link = document.createElement("a");
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            link.remove();
        }
        setTimeout(() => URL.revokeObjectURL(url), 60000);
    }
};


