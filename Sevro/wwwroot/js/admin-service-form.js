// Admin Catalog Service Form: disable price input when PricingType is QuoteRequired
document.addEventListener("DOMContentLoaded", function () {
    const typeSel = document.getElementById("adminPricingType");
    const priceInput = document.getElementById("adminPriceInput");
    const priceWrap = document.getElementById("adminPriceWrap");

    if (!typeSel || !priceInput || !priceWrap) return;

    function syncQuoteMode() {
        const isQuote = typeSel.value === "QuoteRequired";
        priceInput.disabled = isQuote;
        priceWrap.style.opacity = isQuote ? "0.5" : "1";
        if (isQuote) priceInput.value = "";
    }

    typeSel.addEventListener("change", syncQuoteMode);
    syncQuoteMode();
});
