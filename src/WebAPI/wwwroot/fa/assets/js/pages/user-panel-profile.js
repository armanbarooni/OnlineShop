/**
 * Profile Page Manager
 * Handles user profile operations including loading data, updating information, and image upload.
 */
const ProfileManager = {
    init: async function () {
        this.initUI();
        await this.checkAuthAndLoad();
    },

    initUI: function () {
        // Dropdown setup
        if (window.utils && typeof window.utils.setupDropdown === 'function') {
            window.utils.setupDropdown('user-dropdown-button', 'user-dropdown-menu');
        } else {
            // Fallback if Utils.setupDropdown not available (though it should be)
            const btn = document.getElementById('user-dropdown-button');
            const menu = document.getElementById('user-dropdown-menu');
            if (btn && menu) {
                btn.onclick = () => {
                    menu.classList.toggle('hidden');
                    const icon = document.getElementById('user-dropdown-icon');
                    if (icon) icon.classList.toggle('rotate-180');
                };
            }
        }

        // Logout Handler
        const handleLogout = (e) => {
            e.preventDefault();
            if (confirm('آیا مطمئن هستید که می‌خواهید خارج شوید؟')) {
                if (window.authService) window.authService.logout();
                else window.location.href = 'login.html';
            }
        };

        // Attach logout to all logout buttons
        const logoutBtns = document.querySelectorAll('#logoutButton');
        if (logoutBtns.length > 0) {
            logoutBtns.forEach(btn => btn.addEventListener('click', handleLogout));
        }

        // Also check for any button carrying the text 'خروج' as a fallback
        document.querySelectorAll('button').forEach(btn => {
            if (btn.textContent.includes('خروج') && !btn.id) {
                btn.addEventListener('click', handleLogout);
            }
        });

        // Mobile Menu / Offcanvas
        window.toggleOffcanvas = function (id) {
            const el = document.getElementById(id);
            const overlay = document.querySelector('.overlay');
            if (el) {
                el.classList.remove('invisible', 'opacity-0', '-translate-x-full');
                el.classList.add('active'); // Helper class if needed
                if (overlay) overlay.classList.remove('hidden');
            }
        };

        window.closeOffcanvas = function () {
            document.querySelectorAll('.offcanvas').forEach(el => {
                el.classList.add('invisible', 'opacity-0', '-translate-x-full');
                el.classList.remove('active');
            });
            const overlay = document.querySelector('.overlay');
            if (overlay) overlay.classList.add('hidden');
        };

        // Search Logic
        const searchInput = document.querySelector('input[placeholder*="جستجو"]');
        if (searchInput) {
            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const q = searchInput.value.trim();
                    if (q) window.location.href = `shop.html?search=${encodeURIComponent(q)}`;
                }
            });
        }

        // Dark Mode Toggle
        if (window.utils && typeof window.utils.initDarkMode === 'function') {
            window.utils.initDarkMode('dark-mode-toggle');
        }

        // Profile Form Submission
        const profileForm = document.getElementById('profileForm');
        if (profileForm) {
            profileForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                await this.handleProfileSubmit(profileForm);
            });
        }

        // Image Upload Helper
        const imgInput = document.getElementById('profileImageInput');
        if (imgInput) {
            imgInput.addEventListener('change', async (e) => {
                await this.handleImageUpload(e.target);
            });
        }
    },

    checkAuthAndLoad: async function () {
        // Auth check
        if (window.authService && !window.authService.isAuthenticated()) {
            window.location.href = 'login.html';
            return;
        }

        // Load Profile Data
        if (window.userProfileService) {
            try {
                if (typeof window.utils !== 'undefined' && window.utils.showLoading) window.utils.showLoading(true);

                const response = await window.userProfileService.getProfile();
                if (response && response.success && response.data) {
                    this.populateUserData(response.data);
                }
            } catch (error) {
                console.error('Error loading profile:', error);
                if (typeof window.utils !== 'undefined' && window.utils.showToast) {
                    window.utils.showToast('خطا در بارگذاری اطلاعات پروفایل', 'error');
                }
            } finally {
                if (typeof window.utils !== 'undefined' && window.utils.hideLoading) window.utils.showLoading(false);
            }
        }
    },

    populateUserData: function (user) {
        // Users might have different field names depending on API version, normalizing here
        const firstName = user.firstName || '';
        const lastName = user.lastName || '';
        const fullName = `${firstName} ${lastName}`.trim() || 'کاربر گرامی';

        // Update User Names in UI
        document.querySelectorAll('[data-user-name="true"]').forEach(el => {
            el.textContent = fullName;
        });

        // Update Form Fields
        const setVal = (id, val) => {
            const el = document.getElementById(id);
            if (el) el.value = val || '';
        };

        setVal('firstName', firstName);
        setVal('lastName', lastName);
        setVal('email', user.email || user.userEmail);
        setVal('phone', user.phoneNumber || user.phone);
        setVal('nationalCode', user.nationalCode);

        // Handle BirthDate
        if (user.birthDate) {
            const dateStr = new Date(user.birthDate).toLocaleDateString('fa-IR');
            setVal('birthDate', dateStr);
        }

        // Update Profile Images
        if (user.profilePictureUrl) {
            const imgs = document.querySelectorAll('img[alt="پروفایل کاربر"]');
            imgs.forEach(img => img.src = user.profilePictureUrl);
        }

        // Update header component with user data (avoid duplicate API call)
        if (window.headerComponent && window.headerComponent.updateUserMenuWithData) {
            window.headerComponent.updateUserMenuWithData(user);
        }
    },

    handleProfileSubmit: async function (form) {
        const btn = form.querySelector('button[type="submit"]');
        const originalText = btn.textContent;

        try {
            btn.textContent = 'در حال ذخیره...';
            btn.disabled = true;

            const formData = new FormData(form);
            // Convert FormData to object
            const data = Object.fromEntries(formData.entries());

            // Call API
            const result = await window.userProfileService.updateProfile(data);

            if (result.success) {
                if (typeof window.utils !== 'undefined' && window.utils.showToast) {
                    window.utils.showToast('تغییرات با موفقیت ذخیره شد.', 'success');
                } else if (typeof window.Utils !== 'undefined' && window.Utils.showToast) {
                    window.Utils.showToast('تغییرات با موفقیت ذخیره شد.', 'success');
                }

                // Refresh data to ensure UI reflects valid state
                await this.checkAuthAndLoad();
            } else {
                const errorMsg = result.error || 'خطا در ذخیره تغییرات';
                if (typeof window.utils !== 'undefined' && window.utils.showToast) {
                    window.utils.showToast(errorMsg, 'error');
                } else if (typeof window.Utils !== 'undefined' && window.Utils.showToast) {
                    window.Utils.showToast(errorMsg, 'error');
                }
            }
        } catch (error) {
            console.error('Update error:', error);
            if (typeof window.utils !== 'undefined' && window.utils.showToast) {
                window.utils.showToast('خطا در ارتباط با سرور.', 'error');
            } else if (typeof window.Utils !== 'undefined' && window.Utils.showToast) {
                window.Utils.showToast('خطا در ارتباط با سرور.', 'error');
            }
        } finally {
            btn.textContent = originalText;
            btn.disabled = false;
        }
    },

    handleImageUpload: async function (input) {
        if (!input.files || !input.files[0]) return;

        const file = input.files[0];
        const img = document.getElementById('profileImage');
        
        // ذخیره تصویر قبلی برای بازگردانی در صورت خطا
        const previousImageSrc = img ? img.src : null;

        // Preview
        const reader = new FileReader();
        reader.onload = function (e) {
            if (img) img.src = e.target.result;
        }
        reader.readAsDataURL(file);

        // Upload
        const btn = document.querySelector('button[onclick*="profileImageInput"]');
        if (btn) btn.disabled = true;

        try {
            if (typeof window.Utils !== 'undefined' && window.Utils.showLoading) window.Utils.showLoading(true);

            const result = await window.userProfileService.uploadProfilePicture(file);

            if (result.success) {
                if (typeof window.utils !== 'undefined' && window.utils.showToast) {
                    window.utils.showToast('تصویر با موفقیت آپلود شد', 'success');
                } else if (typeof window.Utils !== 'undefined' && window.Utils.showToast) {
                    window.Utils.showToast('تصویر با موفقیت آپلود شد', 'success');
                }

                if (result.data && result.data.imageUrl) {
                    // Update all profile images
                    document.querySelectorAll('img[alt="پروفایل کاربر"]').forEach(img => img.src = result.data.imageUrl);
                }
            } else {
                // بازگردانی تصویر قبلی در صورت خطا
                if (img && previousImageSrc) {
                    img.src = previousImageSrc;
                }
                
                const errorMsg = result.error || 'خطا در آپلود تصویر';
                if (typeof window.utils !== 'undefined' && window.utils.showToast) {
                    window.utils.showToast(errorMsg, 'error');
                } else if (typeof window.Utils !== 'undefined' && window.Utils.showToast) {
                    window.Utils.showToast(errorMsg, 'error');
                }
            }
        } catch (err) {
            console.error(err);
            
            // بازگردانی تصویر قبلی در صورت خطا
            if (img && previousImageSrc) {
                img.src = previousImageSrc;
            }
            
            const errorMsg = err.message || 'خطا در آپلود تصویر';
            if (typeof window.utils !== 'undefined' && window.utils.showToast) {
                window.utils.showToast(errorMsg, 'error');
            } else if (typeof window.Utils !== 'undefined' && window.Utils.showToast) {
                window.Utils.showToast(errorMsg, 'error');
            }
        } finally {
            if (btn) btn.disabled = false;
            if (typeof window.Utils !== 'undefined' && window.Utils.hideLoading) window.Utils.showLoading(false);
            // پاک کردن مقدار input برای امکان انتخاب مجدد همان فایل
            input.value = '';
        }
    }
};

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    // Note: Dependencies (auth-service, user-profile-service, utils) must be loaded before this script
    ProfileManager.init();
});
