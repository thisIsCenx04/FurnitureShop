// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
console.log("site.js loaded");
// Toggle category tree inside bootstrap dropdown
// ===== Category tree toggle (works inside Bootstrap dropdown/navbar collapse) =====
// Prevent dropdown from closing when clicking inside
document.addEventListener("click", function (e) {
    if (e.target.closest(".dropdown-menu")) {
        e.stopPropagation();
    }
});

// Toggle category children (works inside dropdown)
document.addEventListener("click", function (e) {
    const toggle = e.target.closest(".cat-toggle");
    const row = e.target.closest(".cat-row");

    // click link thì cho đi link
    if (e.target.closest("a.cat-link")) return;

    if (!toggle && !row) return;

    const item = (toggle || row).closest(".cat-item");
    if (!item) return;

    const children = item.querySelector(":scope > ul.cat-children");
    if (!children) return;

    e.preventDefault();
    e.stopPropagation();

    const isOpen = item.classList.toggle("open");
    children.style.display = isOpen ? "block" : "none";
}, true);

document.addEventListener("click", function (e) {
    const btn = e.target.closest(".qty-btn");
    if (!btn) return;

    const cartItemId = btn.dataset.id;
    const delta = parseInt(btn.dataset.delta, 10);

    const input = btn.parentElement.querySelector("input");
    let qty = parseInt(input.value, 10) + delta;

    if (qty < 1) qty = 1;

    fetch("/cart/update", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: `cartItemId=${cartItemId}&qty=${qty}`
    })
        .then(() => location.reload());
});
