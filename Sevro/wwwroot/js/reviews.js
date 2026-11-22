// Interactive 5-star rating selector inside Customer Review Modal
document.addEventListener("DOMContentLoaded", function () {
    const reviewModal = document.getElementById("reviewModal");
    if (!reviewModal) return;

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
});
