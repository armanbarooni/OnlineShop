(function () {
  "use strict";

  function initIndexSlider() {
    if (typeof window.Swiper === "undefined") return;

    const sliders = document.querySelectorAll(".default-carousel");
    if (!sliders.length) return;

    sliders.forEach((slider) => {
      if (slider.classList.contains("swiper-initialized")) return;

      const slideCount = slider.querySelectorAll(".swiper-slide").length;
      const nextButton =
        slider.querySelector(".custom-swiper-next") ||
        slider.querySelector(".swiper-button-next");
      const prevButton =
        slider.querySelector(".custom-swiper-prev") ||
        slider.querySelector(".swiper-button-prev");

      const swiper = new Swiper(slider, {
        loop: slideCount > 1,
        initialSlide: 0,
        speed: 600,
        autoplay:
          slideCount > 1
            ? {
                delay: 4500,
                disableOnInteraction: false,
                pauseOnMouseEnter: true,
              }
            : false,
        grabCursor: true,
        allowTouchMove: true,
        simulateTouch: true,
        watchOverflow: true,
        observer: true,
        observeParents: true,
        resizeObserver: true,
        pagination: {
          el: slider.querySelector(".swiper-pagination"),
          clickable: true,
        },
        navigation: {
          nextEl: nextButton,
          prevEl: prevButton,
        },
      });
      if (slideCount > 1) {
        swiper.slideToLoop(0, 0, false);
      }

      slider.setAttribute("data-component-ready", "index-slider");
      slider.swiper = swiper;
    });
  }

  function scheduleInit() {
    window.requestAnimationFrame(initIndexSlider);
  }

  if (document.readyState === "complete") {
    scheduleInit();
  } else {
    window.addEventListener("layout:ready", scheduleInit, { once: true });
    window.addEventListener("components:ready", scheduleInit, { once: true });
    window.addEventListener("DOMContentLoaded", scheduleInit, { once: true });
    window.addEventListener("load", scheduleInit, { once: true });
  }
})();
