(function () {
  "use strict";

  const loadedScripts = new Set();

  function fetchScript(src) {
    const request = new XMLHttpRequest();
    request.open("GET", src, false);
    request.send(null);

    if (
      (request.status >= 200 && request.status < 300) ||
      (request.status === 0 && request.responseText)
    ) {
      return request.responseText;
    }

    throw new Error("Could not load component script: " + src);
  }

  function runScript(src) {
    if (!src || loadedScripts.has(src)) return;
    loadedScripts.add(src);

    const script = document.createElement("script");
    script.text = fetchScript(src) + "\n//# sourceURL=" + src;
    document.body.appendChild(script);
  }

  function loadComponentScripts() {
    const scripts = window.layoutComponentScripts || [];

    scripts.forEach(function (script) {
      try {
        runScript(script);
      } catch (error) {
        if (window.logger && typeof window.logger.error === "function") {
          window.logger.error(error.message, error);
        } else {
          console.error(error);
        }
      }
    });

    window.dispatchEvent(new CustomEvent("components:ready"));
  }

  loadComponentScripts();
})();
