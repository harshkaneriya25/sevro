// Reusable table & card grid pagination ([data-sc-paginate])
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("[data-sc-paginate]").forEach((container) => {
        const defaultSize = parseInt(container.getAttribute("data-sc-paginate"), 10) || 8;
        const itemSelector = container.getAttribute("data-sc-item") || (container.tagName === "TBODY" ? ":scope > tr" : ':scope > [class*="col"], :scope > .sc-page-item');
        const items = Array.from(container.querySelectorAll(itemSelector));
        if (items.length === 0) return;

        let pageSize = defaultSize;
        let currentPage = 1;

        const bar = document.createElement("div");
        const isTable = container.tagName === "TBODY";
        bar.className = isTable ? "sc-pagination-bar" : "sc-pagination-bar sc-pagination-standalone";

        const infoEl = document.createElement("div");
        infoEl.className = "sc-page-info";

        const rightWrap = document.createElement("div");
        rightWrap.className = "d-flex flex-wrap align-items-center gap-2";

        const sizeSelect = document.createElement("select");
        sizeSelect.className = "form-select form-select-sm sc-page-size";
        sizeSelect.setAttribute("aria-label", "Items per page");
        const sizeOptions = Array.from(new Set([defaultSize, 12, 24, 50])).sort((a, b) => a - b);
        sizeOptions.forEach((n) => {
            const opt = document.createElement("option");
            opt.value = String(n);
            opt.textContent = `${n} / page`;
            if (n === pageSize) opt.selected = true;
            sizeSelect.appendChild(opt);
        });

        const controlsEl = document.createElement("div");
        controlsEl.className = "sc-page-controls";
        controlsEl.setAttribute("role", "navigation");
        controlsEl.setAttribute("aria-label", "Pagination");

        rightWrap.appendChild(sizeSelect);
        rightWrap.appendChild(controlsEl);
        bar.appendChild(infoEl);
        bar.appendChild(rightWrap);

        if (isTable) {
            const tableWrap = container.closest(".table-responsive") || container.closest("table");
            if (tableWrap && tableWrap.parentNode) {
                tableWrap.parentNode.insertBefore(bar, tableWrap.nextSibling);
            }
        } else if (container.parentNode) {
            container.parentNode.insertBefore(bar, container.nextSibling);
        }

        function renderPage(scrollIntoView) {
            const total = items.length;
            const totalPages = Math.max(1, Math.ceil(total / pageSize));
            if (currentPage > totalPages) currentPage = totalPages;

            const startIdx = (currentPage - 1) * pageSize;
            const endIdx = Math.min(startIdx + pageSize, total);

            items.forEach((el, idx) => {
                const visible = idx >= startIdx && idx < endIdx;
                el.style.display = visible ? "" : "none";
            });

            infoEl.innerHTML = `Showing <strong>${startIdx + 1}&ndash;${endIdx}</strong> of <strong>${total}</strong> ${total === 1 ? "item" : "items"}`;
            controlsEl.innerHTML = "";

            const prevBtn = document.createElement("button");
            prevBtn.type = "button";
            prevBtn.className = "sc-page-btn";
            prevBtn.disabled = currentPage <= 1;
            prevBtn.setAttribute("aria-label", "Previous page");
            prevBtn.innerHTML = `<i class="bi bi-chevron-left" aria-hidden="true"></i>`;
            prevBtn.addEventListener("click", () => {
                if (currentPage > 1) {
                    currentPage--;
                    renderPage(true);
                }
            });
            controlsEl.appendChild(prevBtn);

            for (let p = 1; p <= totalPages; p++) {
                if (totalPages > 7 && p !== 1 && p !== totalPages && Math.abs(p - currentPage) > 1) {
                    if (p === 2 || p === totalPages - 1) {
                        const dots = document.createElement("span");
                        dots.className = "px-1 text-muted small";
                        dots.textContent = "…";
                        controlsEl.appendChild(dots);
                    }
                    continue;
                }
                const btn = document.createElement("button");
                btn.type = "button";
                btn.className = `sc-page-btn ${p === currentPage ? "active" : ""}`;
                btn.textContent = String(p);
                btn.setAttribute("aria-current", p === currentPage ? "page" : "false");
                btn.addEventListener("click", () => {
                    currentPage = p;
                    renderPage(true);
                });
                controlsEl.appendChild(btn);
            }

            const nextBtn = document.createElement("button");
            nextBtn.type = "button";
            nextBtn.className = "sc-page-btn";
            nextBtn.disabled = currentPage >= totalPages;
            nextBtn.setAttribute("aria-label", "Next page");
            nextBtn.innerHTML = `<i class="bi bi-chevron-right" aria-hidden="true"></i>`;
            nextBtn.addEventListener("click", () => {
                if (currentPage < totalPages) {
                    currentPage++;
                    renderPage(true);
                }
            });
            controlsEl.appendChild(nextBtn);

            if (scrollIntoView) {
                const anchor = isTable ? (container.closest(".sc-card") || container) : container;
                anchor.scrollIntoView({ behavior: "smooth", block: "start" });
            }
        }

        sizeSelect.addEventListener("change", () => {
            pageSize = parseInt(sizeSelect.value, 10) || defaultSize;
            currentPage = 1;
            renderPage(false);
        });

        renderPage(false);
    });
});
