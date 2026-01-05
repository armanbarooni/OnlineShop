/**
 * Navbar Fix - Apply consistent navbar changes across all pages
 * This script ensures:
 * 1. Remove location filter section
 * 2. Remove menu items (لیست کالاها, سوالی دارید, پیگیری سفارش)
 * 3. Set static cart count to 2
 * 4. Fix garbled text in mega menu
 */

(function() {
    'use strict';

    // Wait for DOM to be ready
    function init() {
        // 1. Remove location filter section
        const locationFilter = document.querySelector('[data-modal-target="filterModal"]');
        if (locationFilter) {
            locationFilter.remove();
        }

        // 2. Remove menu items
        const menuItemsToRemove = [
            'لیست کالا ها',
            'لیست کالاها',
            'سوالی دارید',
            'پیگیری سفارش'
        ];

        const menuItems = document.querySelectorAll('#megaMenu ul li');
        menuItems.forEach(li => {
            const link = li.querySelector('a');
            if (link) {
                const linkText = link.textContent.trim();
                menuItemsToRemove.forEach(itemText => {
                    if (linkText.includes(itemText)) {
                        li.remove();
                    }
                });
            }
        });

        // 3. Fix garbled text in mega menu
        const megaMenuTexts = document.querySelectorAll('#mega-menu-fire-target .col-span-10 p, #mega-menu-fire-target p.text-gray-500');
        megaMenuTexts.forEach(text => {
            if (text.textContent.includes('ÛŒÚ©') || text.textContent.includes('Ø¯Ø³ØªÙ‡')) {
                text.textContent = 'یک دسته بندی را انتخاب کنید';
            }
        });

        // 4. Set static cart count to 2
        function setCartCount() {
            const cartBadges = document.querySelectorAll('span.size-5.text-sm.-top-3.-end-2');
            cartBadges.forEach(badge => {
                badge.textContent = '2';
            });
        }

        // Set immediately
        setCartCount();

        // Set periodically to prevent updates
        setInterval(setCartCount, 1000);
    }

    // Run when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

