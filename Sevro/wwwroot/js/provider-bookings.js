// Provider Bookings: populate Send Quotation modal fields
document.addEventListener("DOMContentLoaded", function () {
    const bookingIdEl = document.getElementById("quoteBookingId");
    const serviceLabelEl = document.getElementById("quoteServiceLabel");
    const amountInput = document.getElementById("quoteAmountInput");
    const notesInput = document.getElementById("quoteNotesInput");

    if (!bookingIdEl) return;

    document.querySelectorAll(".js-open-quote-modal").forEach((btn) => {
        btn.addEventListener("click", () => {
            const id = btn.getAttribute("data-booking-id") || "";
            const name = btn.getAttribute("data-service-name") || "";
            bookingIdEl.value = id;
            if (serviceLabelEl) serviceLabelEl.textContent = `Booking #${id} — ${name}`;
            if (amountInput) amountInput.value = btn.getAttribute("data-current-quote") || "";
            if (notesInput) notesInput.value = btn.getAttribute("data-current-notes") || "";
        });
    });
});
