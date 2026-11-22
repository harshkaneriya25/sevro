// Navbar scroll elevation, sliding pill indicator, and scroll-reveal animations
document.addEventListener("DOMContentLoaded", function () {
    const siteNavbar = document.getElementById("mainSiteNavbar");
    if (siteNavbar) {
        const syncNavbarScroll = () => {
            siteNavbar.classList.toggle("is-scrolled", window.scrollY > 12);
        };
        syncNavbarScroll();
        window.addEventListener("scroll", syncNavbarScroll, { passive: true });
    }

    const navDock = document.getElementById("scNavDock");
    const navSlider = document.getElementById("scNavSlider");
    if (navDock && navSlider) {
        const navLinks = Array.from(navDock.querySelectorAll(".nav-link"));

        function moveSliderTo(target) {
            if (!target || window.innerWidth < 992) {
                navSlider.classList.remove("is-ready");
                return;
            }
            const dockRect = navDock.getBoundingClientRect();
            const linkRect = target.getBoundingClientRect();
            const offsetX = linkRect.left - dockRect.left;
            navSlider.style.width = `${linkRect.width}px`;
            navSlider.style.transform = `translateX(${offsetX}px)`;
            navSlider.classList.add("is-ready");
        }

        const activeLink = navDock.querySelector(".nav-link.active");
        if (activeLink) {
            requestAnimationFrame(() => moveSliderTo(activeLink));
            if (document.fonts && document.fonts.ready) {
                document.fonts.ready.then(() => moveSliderTo(activeLink));
            }
        }

        navLinks.forEach((link) => {
            link.addEventListener("mouseenter", () => moveSliderTo(link));
            link.addEventListener("focus", () => moveSliderTo(link));
        });

        navDock.addEventListener("mouseleave", () => {
            if (activeLink) {
                moveSliderTo(activeLink);
            } else {
                navSlider.classList.remove("is-ready");
            }
        });

        window.addEventListener("resize", () => {
            const currentActive = navDock.querySelector(".nav-link.active");
            if (currentActive) {
                moveSliderTo(currentActive);
            } else {
                navSlider.classList.remove("is-ready");
            }
        }, { passive: true });
    }

    // Subtle Scroll Reveal (respects prefers-reduced-motion)
    const revealEls = document.querySelectorAll(".sc-reveal");
    const reducedMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (revealEls.length && !reducedMotion && "IntersectionObserver" in window) {
        document.documentElement.classList.add("sc-anim-ready");
        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.08, rootMargin: "0px 0px -30px 0px" });

        revealEls.forEach((el) => observer.observe(el));
    }
});
