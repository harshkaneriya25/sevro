document.addEventListener("DOMContentLoaded", function () {
    // 1. Interactive 5-Star Rating Selector inside Review Modal
    const reviewModal = document.getElementById("reviewModal");
    if (reviewModal) {
        const bookingIdInput = reviewModal.querySelector("#reviewBookingId");
        const serviceTitleEl = reviewModal.querySelector("#reviewServiceName");
        const ratingInput = reviewModal.querySelector("#reviewRatingInput");
        const ratingLabel = reviewModal.querySelector("#reviewRatingLabel");
        const commentInput = reviewModal.querySelector("#reviewCommentInput");
        const starButtons = reviewModal.querySelectorAll(".sc-star-btn");

        const ratingDescriptions = {
            1: "1 / 5 — Poor",
            2: "2 / 5 — Fair",
            3: "3 / 5 — Good",
            4: "4 / 5 — Very Good",
            5: "5 / 5 — Excellent"
        };

        function paintStars(value) {
            starButtons.forEach((btn) => {
                const starVal = parseInt(btn.getAttribute("data-value"), 10);
                btn.classList.toggle("active", starVal <= value);
            });
            if (ratingInput) ratingInput.value = value;
            if (ratingLabel) ratingLabel.textContent = ratingDescriptions[value] || "";
        }

        starButtons.forEach((btn) => {
            btn.addEventListener("click", function () {
                const val = parseInt(this.getAttribute("data-value"), 10);
                paintStars(val);
            });
        });

        reviewModal.addEventListener("show.bs.modal", function (event) {
            const trigger = event.relatedTarget;
            if (!trigger) return;
            const bookingId = trigger.getAttribute("data-booking-id");
            const serviceName = trigger.getAttribute("data-service-name");
            if (bookingIdInput) bookingIdInput.value = bookingId || "";
            if (serviceTitleEl) serviceTitleEl.textContent = serviceName || "Service";
            if (commentInput) commentInput.value = "";
            paintStars(5);
        });
    }

    // 2. Register Page Role Tabs (Customer vs Provider)
    const roleTabs = document.querySelectorAll("[data-role-tab]");
    const roleHiddenInput = document.getElementById("registerRoleInput");
    const providerFieldsSection = document.getElementById("providerFieldsSection");
    if (roleTabs.length && roleHiddenInput && providerFieldsSection) {
        function syncRoleUI(role) {
            roleHiddenInput.value = role;
            const isProvider = role === "Provider";
            providerFieldsSection.classList.toggle("d-none", !isProvider);
            roleTabs.forEach((tab) => {
                const active = tab.getAttribute("data-role-tab") === role;
                tab.classList.toggle("active", active);
            });
        }

        roleTabs.forEach((tab) => {
            tab.addEventListener("click", function (e) {
                e.preventDefault();
                syncRoleUI(this.getAttribute("data-role-tab"));
            });
        });

        syncRoleUI(roleHiddenInput.value || "Customer");
    }

    // 3. Auto-submit Availability Switch forms for Providers
    document.querySelectorAll(".js-availability-switch").forEach((sw) => {
        sw.addEventListener("change", function () {
            const form = this.closest("form");
            if (form) form.submit();
        });
    });

    // 4. Auto-submit Catalog Filter changes
    document.querySelectorAll(".js-auto-submit").forEach((el) => {
        el.addEventListener("change", function () {
            const form = this.closest("form");
            if (form) form.submit();
        });
    });

    // 5. Password Show / Hide Eye Toggle
    document.querySelectorAll(".sc-password-toggle").forEach((btn) => {
        btn.addEventListener("click", function () {
            const wrapper = this.closest(".sc-password-wrapper");
            if (!wrapper) return;
            const input = wrapper.querySelector("input");
            const icon = this.querySelector("i");
            if (!input) return;

            const isHidden = input.type === "password";
            input.type = isHidden ? "text" : "password";
            if (icon) {
                icon.classList.toggle("bi-eye", !isHidden);
                icon.classList.toggle("bi-eye-slash", isHidden);
            }
            this.setAttribute("aria-label", isHidden ? "Hide password" : "Show password");
        });
    });
});
