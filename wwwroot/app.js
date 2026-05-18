window.setupSlider = function(options) {
    const slider = document.getElementById(options.sliderId);
    if (!slider) return;

    let lastSent = 0;
    let timeoutId = null;
    let pendingValue = null;

    function sendValue(val) {
        lastSent = Date.now();
        pendingValue = null;
        options.dotNetRef.invokeMethodAsync(options.methodName, val);
    }

    function scheduleSend(val) {
        const now = Date.now();
        const timeSinceLast = now - lastSent;

        if (timeSinceLast >= options.throttleMs) {
            sendValue(val);
        } else {
            // schedule latest value after remaining time
            pendingValue = val;
            if (timeoutId) clearTimeout(timeoutId);
            timeoutId = setTimeout(() => {
                sendValue(pendingValue);
                timeoutId = null;
            }, options.throttleMs - timeSinceLast);
        }
    }

    slider.addEventListener("input", () => {
        const val = parseInt(slider.value, 10);
        scheduleSend(val);
    });

    slider.addEventListener("change", () => {
        const val = parseInt(slider.value, 10);
        if (timeoutId) clearTimeout(timeoutId); // cancel any pending throttled update
        sendValue(val); // force send immediately on release
    });
};

window.fixGaugeText = function() {
  document.querySelectorAll(".gauge-edit-title").forEach(el => {
    const c = getComputedStyle(el).backgroundColor.match(/\d+/g).map(Number);
    const L = 0.2126*c[0] + 0.7152*c[1] + 0.0722*c[2];
    el.style.color = L > 160 ? "black" : "white";
  });
};
