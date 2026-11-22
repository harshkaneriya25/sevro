// Catalog filter auto-submit & provider availability switch toggle
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".js-availability-switch").forEach((sw) => {
        sw.addEventListener("change", function () {
            const form = this.closest("form");
            if (form) form.submit();
        });
    });

    document.querySelectorAll(".js-auto-submit").forEach((el) => {
        el.addEventListener("change", function () {
            const form = this.closest("form");
            if (form) form.submit();
        });
    });
});
