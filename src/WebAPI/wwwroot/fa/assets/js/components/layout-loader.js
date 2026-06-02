(function () {
  "use strict";

  const PARTIAL_BASE_PATH = "partials/";
  const componentScripts = [];
  const loadedScripts = new Set();

  function getPartialName(element) {
    return element.getAttribute("data-layout-partial");
  }

  function collectComponentScripts(element) {
    const scripts = element.getAttribute("data-layout-script");
    if (!scripts) return;

    scripts
      .split(",")
      .map(function (script) {
        return script.trim();
      })
      .filter(Boolean)
      .forEach(function (script) {
        if (!componentScripts.includes(script)) {
          componentScripts.push(script);
        }
      });
  }

  function loadPartial(name) {
    const request = new XMLHttpRequest();
    request.open("GET", PARTIAL_BASE_PATH + name + ".html", false);
    request.send(null);

    if (
      (request.status >= 200 && request.status < 300) ||
      (request.status === 0 && request.responseText)
    ) {
      return request.responseText;
    }

    throw new Error("Could not load layout partial: " + name);
  }

  function loadScriptOnce(src) {
    if (!src || loadedScripts.has(src)) return;
    loadedScripts.add(src);

    const script = document.createElement("script");
    script.src = src;
    script.defer = true;
    document.body.appendChild(script);
  }

  function applyPartials() {
    const placeholders = document.querySelectorAll("[data-layout-partial]");

    placeholders.forEach(function (placeholder) {
      const partialName = getPartialName(placeholder);
      if (!partialName) return;

      try {
        collectComponentScripts(placeholder);
        placeholder.outerHTML = loadPartial(partialName);
        if (partialName === "site-header") {
          loadScriptOnce("assets/js/components/site-header-auth.js");
        }
      } catch (error) {
        if (window.logger && typeof window.logger.error === "function") {
          window.logger.error(error.message, error);
        } else {
          console.error(error);
        }
      }
    });

    window.layoutComponentScripts = componentScripts.slice();
    window.dispatchEvent(new CustomEvent("layout:ready"));
  }

  applyPartials();

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", applyPartials, { once: true });
  }
})();
