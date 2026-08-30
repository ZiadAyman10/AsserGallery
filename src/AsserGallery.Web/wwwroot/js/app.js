// Asser Gallery — Client Interactive Scripts

(function () {
    // 1. Theme Initializer & Toggle
    const savedTheme = localStorage.getItem('asser_theme') || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    document.documentElement.setAttribute('data-theme', savedTheme);

    window.toggleTheme = function () {
        const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
        const nextTheme = currentTheme === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', nextTheme);
        localStorage.setItem('asser_theme', nextTheme);
        document.cookie = `theme=${nextTheme};path=/;max-age=31536000`;
        updateThemeIcons(nextTheme);
    };

    function updateThemeIcons(theme) {
        document.querySelectorAll('.theme-icon-sun').forEach(el => el.style.display = theme === 'dark' ? 'inline-block' : 'none');
        document.querySelectorAll('.theme-icon-moon').forEach(el => el.style.display = theme === 'dark' ? 'none' : 'inline-block');
    }

    document.addEventListener('DOMContentLoaded', () => {
        updateThemeIcons(document.documentElement.getAttribute('data-theme'));

        // Realtime Search Debounce in Catalog
        const searchInput = document.getElementById('catalogSearchInput');
        const filterForm = document.getElementById('catalogFilterForm');
        if (searchInput && filterForm) {
            let debounceTimeout;
            searchInput.addEventListener('input', () => {
                clearTimeout(debounceTimeout);
                debounceTimeout = setTimeout(() => {
                    filterForm.submit();
                }, 500);
            });
        }
    });

    // 2. Clipboard Helper
    window.copyToClipboard = function (elementId, buttonElement) {
        const target = document.getElementById(elementId);
        if (!target) return;
        const text = target.value || target.innerText;
        navigator.clipboard.writeText(text).then(() => {
            const origHtml = buttonElement.innerHTML;
            const isAr = document.documentElement.getAttribute('lang') === 'ar';
            buttonElement.innerHTML = isAr ? '✅ تم النسخ!' : '✅ Copied!';
            buttonElement.classList.add('btn-success');
            setTimeout(() => {
                buttonElement.innerHTML = origHtml;
                buttonElement.classList.remove('btn-success');
            }, 2000);
        });
    };

    // 3. Product Image Toggle (Original vs AI-Enhanced)
    window.switchProductImage = function (imgUrl, button, modeName) {
        const mainImg = document.getElementById('mainProductDisplayImage');
        if (mainImg) {
            mainImg.style.opacity = '0.3';
            setTimeout(() => {
                mainImg.src = imgUrl;
                mainImg.style.opacity = '1';
            }, 150);
        }

        if (button && button.parentElement) {
            button.parentElement.querySelectorAll('.image-mode-btn').forEach(btn => btn.classList.remove('active'));
            button.classList.add('active');
        }
    };

    // 4. Color Variant Selection on Details Page
    window.selectColorVariant = function (variantId, colorName, quantity, price, productName, storeWhatsApp, lang) {
        document.querySelectorAll('.color-variant-chip').forEach(el => el.classList.remove('active'));
        const activeChip = document.getElementById('variant-chip-' + variantId);
        if (activeChip) activeChip.classList.add('active');

        const stockNotice = document.getElementById('selectedVariantStockNotice');
        if (stockNotice) {
            if (quantity > 3) {
                stockNotice.innerHTML = lang === 'ar' ? `متوفر (${quantity} قطعة)` : `In Stock (${quantity} pcs)`;
                stockNotice.className = 'status-badge status-available';
            } else if (quantity > 0) {
                stockNotice.innerHTML = lang === 'ar' ? `كمية محدودة! (${quantity} متبقي)` : `Limited Stock! (${quantity} left)`;
                stockNotice.className = 'status-badge status-limited';
            } else {
                stockNotice.innerHTML = lang === 'ar' ? `نفد هذا اللون` : `Out of Stock`;
                stockNotice.className = 'status-badge status-outofstock';
            }
        }

        // Update WhatsApp Order Button URL
        const waBtn = document.getElementById('btnOrderWhatsApp');
        if (waBtn) {
            const currentUrl = window.location.href;
            let msg = '';
            if (lang === 'ar') {
                msg = `مرحباً آسر جاليري 👋، أود طلب المنتج:\n✨ *${productName}*\n🎨 اللون: *${colorName}*\n💰 السعر: *${price} ج.م*\n🔗 الرابط: ${currentUrl}`;
            } else {
                msg = `Hello Asser Gallery 👋, I would like to order:\n✨ *${productName}*\n🎨 Color: *${colorName}*\n💰 Price: *${price} EGP*\n🔗 Link: ${currentUrl}`;
            }
            waBtn.href = `https://wa.me/${storeWhatsApp}?text=${encodeURIComponent(msg)}`;
        }
    };
})();
