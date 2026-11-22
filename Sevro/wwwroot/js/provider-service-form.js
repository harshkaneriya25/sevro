// Provider Create/Edit Service Form: Catalog template quick-fill & QuoteRequired pricing toggle
document.addEventListener("DOMContentLoaded", function () {
    const catSelect = document.getElementById("providerCategorySelect");
    const tmplSelect = document.getElementById("catalogTemplateSelect");
    const nameInput = document.getElementById("serviceNameInput");
    const descInput = document.getElementById("descriptionInput");
    const priceInput = document.getElementById("priceInput");
    const typeSelect = document.getElementById("pricingTypeSelect");
    const unitInput = document.getElementById("unitInput");
    const durationInput = document.getElementById("durationInput");
    const priceHint = document.getElementById("priceHint");

    if (!catSelect || !tmplSelect || !typeSelect) return;

    let catalogCache = [];

    function syncQuoteMode() {
        const isQuote = typeSelect.value === "QuoteRequired";
        if (isQuote) {
            priceInput.value = "0";
            priceInput.readOnly = true;
            priceInput.classList.add("bg-light", "text-muted");
            if (priceHint) priceHint.textContent = "No upfront price charged — send a quotation after inspecting the job.";
        } else {
            priceInput.readOnly = false;
            priceInput.classList.remove("bg-light", "text-muted");
            if (priceHint) priceHint.textContent = "Displayed transparently to customers before booking.";
        }
    }

    async function loadTemplates(categoryId, preselectId) {
        tmplSelect.innerHTML = '<option value="">-- Custom Service (or Quick-Fill) --</option>';
        catalogCache = [];
        if (!categoryId) return;

        try {
            const res = await fetch("/Provider/CatalogServicesByCategory?categoryId=" + encodeURIComponent(categoryId));
            if (!res.ok) return;
            catalogCache = await res.json();
            for (const item of catalogCache) {
                const opt = document.createElement("option");
                opt.value = item.id;
                const labelPrice = item.pricingType === "QuoteRequired"
                    ? "Get Quote"
                    : "₹" + Number(item.price).toLocaleString("en-IN");
                opt.textContent = `${item.name} (${labelPrice})`;
                if (preselectId && String(item.id) === String(preselectId)) {
                    opt.selected = true;
                }
                tmplSelect.appendChild(opt);
            }
        } catch {
            // Provider can still enter a custom service if lookup fails
        }
    }

    catSelect.addEventListener("change", () => loadTemplates(catSelect.value, null));

    tmplSelect.addEventListener("change", () => {
        const selected = catalogCache.find((x) => String(x.id) === tmplSelect.value);
        if (!selected) return;
        nameInput.value = selected.name;
        descInput.value = selected.description || "";
        typeSelect.value = selected.pricingType;
        priceInput.value = selected.pricingType === "QuoteRequired" ? "0" : selected.price;
        unitInput.value = selected.unit || "Per job";
        durationInput.value = selected.duration || "30–60 mins";
        syncQuoteMode();
    });

    typeSelect.addEventListener("change", syncQuoteMode);

    syncQuoteMode();
    if (catSelect.value) {
        loadTemplates(catSelect.value, tmplSelect.getAttribute("data-selected"));
    }
});
