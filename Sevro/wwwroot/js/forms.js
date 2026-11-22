// Form interactions: role tabs, password toggle, unobtrusive validation UX & loading submit
document.addEventListener("DOMContentLoaded", function () {
    // 1. Register Page Role Tabs (Customer vs Provider)
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

    // 2. Password Show / Hide Eye Toggle
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

    // 3. Modern Form Validation UX (jQuery Unobtrusive Bridge + Shake + Loading Submit)
    const scHighlight = function (element) {
        element.classList.add("input-validation-error", "is-invalid");
        element.classList.remove("sc-field-valid");
    };
    const scUnhighlight = function (element) {
        element.classList.remove("input-validation-error", "is-invalid");
        if (element.value && element.value.trim().length > 0 && element.type !== "hidden") {
            element.classList.add("sc-field-valid");
        } else {
            element.classList.remove("sc-field-valid");
        }
    };

    if (window.jQuery && window.jQuery.validator) {
        window.jQuery.validator.setDefaults({
            highlight: scHighlight,
            unhighlight: scUnhighlight
        });
    }

    document.querySelectorAll("form[method='post']").forEach((form) => {
        if (window.jQuery) {
            const existingValidator = window.jQuery(form).data("validator");
            if (existingValidator && existingValidator.settings) {
                existingValidator.settings.highlight = scHighlight;
                existingValidator.settings.unhighlight = scUnhighlight;
            }
        }

        const serverInvalid = form.querySelectorAll(".input-validation-error");
        serverInvalid.forEach((el) => el.classList.add("is-invalid"));

        form.querySelectorAll("input, select, textarea").forEach((field) => {
            field.addEventListener("input", () => {
                if (field.classList.contains("input-validation-error") && field.value.trim().length > 0) {
                    if (window.jQuery && window.jQuery(form).data("validator")) {
                        window.jQuery(form).validate().element(field);
                    }
                }
            });
        });

        form.addEventListener("submit", (event) => {
            const $form = window.jQuery ? window.jQuery(form) : null;
            const hasValidator = $form && $form.data("validator");

            if (hasValidator && !$form.valid()) {
                const firstInvalid = form.querySelector(".input-validation-error, .is-invalid");
                if (firstInvalid) {
                    firstInvalid.classList.remove("sc-shake");
                    void firstInvalid.offsetWidth;
                    firstInvalid.classList.add("sc-shake");
                    firstInvalid.focus({ preventScroll: false });
                }
                return;
            }

            if (event.defaultPrevented) return;

            const submitBtn = form.querySelector("button[type='submit']:not([disabled])");
            if (submitBtn && !submitBtn.classList.contains("dropdown-item")) {
                submitBtn.classList.add("disabled");
                submitBtn.setAttribute("aria-busy", "true");
                const originalWidth = submitBtn.offsetWidth;
                if (originalWidth > 90) {
                    submitBtn.style.minWidth = `${originalWidth}px`;
                }
                submitBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span><span>Please wait...</span>`;
            }
        });
    });
});
