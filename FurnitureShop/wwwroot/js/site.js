console.log("✨ FurnitureShop loaded");

// ============================================
// SMOOTH ANIMATIONS & INTERACTIONS
// ============================================

document.addEventListener("DOMContentLoaded", function () {
    // Initialize Bootstrap Dropdowns
    document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach(el => {
        bootstrap.Dropdown.getOrCreateInstance(el);
    });

    // Add ripple effect to buttons
    addRippleEffect();
    
    // Lazy load images
    lazyLoadImages();
    
    // Add loading state to forms
    enhanceForms();
});

// ============================================
// CATEGORY DROPDOWN LOGIC
// ============================================

document.addEventListener("click", function (e) {
    // Prevent dropdown close when clicking inside category menu
    if (e.target.closest(".category-dropdown .dropdown-menu")) {
        e.stopPropagation();
    }
});

document.addEventListener("click", function (e) {
    if (!e.target.closest(".category-dropdown")) return;
    if (e.target.closest("a.cat-link")) return;

    const toggle = e.target.closest(".cat-toggle");
    const row = e.target.closest(".cat-row");
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

// ============================================
// CART QUANTITY BUTTONS
// ============================================

function getAntiForgeryToken() {
    const el = document.querySelector('#af-token input[name="__RequestVerificationToken"]');
    return el ? el.value : "";
}

document.addEventListener("click", function (e) {
    const btn = e.target.closest(".qty-btn");
    if (!btn) return;

    const cartItemId = btn.dataset.id;
    const delta = parseInt(btn.dataset.delta, 10);

    const input = btn.parentElement.querySelector("input");
    let qty = parseInt(input.value, 10) + delta;
    if (qty < 1) qty = 1;

    const token = getAntiForgeryToken();

    // Show loading state
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

    fetch("/cart/update", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            "RequestVerificationToken": token
        },
        body: `cartItemId=${encodeURIComponent(cartItemId)}&qty=${encodeURIComponent(qty)}`
    })
    .then(response => {
        if (response.ok) {
            location.reload();
        } else {
            throw new Error('Update failed');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        btn.disabled = false;
        btn.innerHTML = delta > 0 ? '+' : '-';
        alert('Có lỗi xảy ra. Vui lòng thử lại.");
    });
});

// ============================================
// RIPPLE EFFECT
// ============================================

function addRippleEffect() {
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple');
            
            this.appendChild(ripple);
            
            setTimeout(() => ripple.remove(), 600);
        });
    });
}

// ============================================
// LAZY LOADING IMAGES
// ============================================

function lazyLoadImages() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                    }
                    observer.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }
}

// ============================================
// FORM ENHANCEMENTS
// ============================================

function enhanceForms() {
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.disabled = true;
                const originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...';
                
                // Re-enable after 5 seconds as fallback
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 5000);
            }
        });
    });
}

// ============================================
// TOAST NOTIFICATIONS
// ============================================

window.showToast = function(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.innerHTML = `
        <div class="d-flex align-items-center gap-2">
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
            <span>${message}</span>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => toast.classList.add('show'), 100);
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
};

// ============================================
// ADD TO CART (Global function)
// ============================================

window.addToCart = async function(productId, event) {
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    
    try {
        const response = await fetch('/cart/add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: `productId=${productId}&qty=1`
        });
        
        if (response.ok) {
            showToast('Đã thêm vào giỏ hàng!', 'success');
            updateCartBadge();
        } else {
            throw new Error('Add to cart failed');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra. Vui lòng thử lại.', 'danger');
    }
};

// ============================================
// UPDATE CART BADGE
// ============================================

function updateCartBadge() {
    // This would need to fetch cart count from server
    // For now, just increment the badge
    const badge = document.getElementById('cartBadge');
    if (badge) {
        const currentCount = parseInt(badge.textContent) || 0;
        badge.textContent = currentCount + 1;
        badge.style.display = 'inline-block';
    }
}

// ============================================
// SCROLL TO TOP ANIMATION
// ============================================

window.addEventListener('scroll', () => {
    const scrolled = window.scrollY;
    if (scrolled > 100) {
        document.body.classList.add('scrolled');
    } else {
        document.body.classList.remove('scrolled');
    }
});

// ============================================
// PRICE FORMATTER
// ============================================

window.formatPrice = function(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
};

// ============================================
// ANIMATE ON SCROLL
// ============================================

if ('IntersectionObserver' in window) {
    const animateOnScroll = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            }
        });
    }, {
        threshold: 0.1
    });

    document.querySelectorAll('.card, .category-card, .product-card').forEach(el => {
        animateOnScroll.observe(el);
    });
}

console.log("✅ All features loaded successfully!");
