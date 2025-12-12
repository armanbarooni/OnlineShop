/**
 * User Panel Address Page Script
 */

const AddressManager = {
    // Current editing address ID (null for create mode)
    editingAddressId: null,

    init() {
        this.initUI();
        this.checkAuthAndLoad();
    },

    initUI() {
        // Dropdowns
        window.utils.setupDropdown('user-dropdown-button', 'user-dropdown-menu');

        // Mobile menu
        const mobileMenuBtn = document.querySelector('button[onclick="toggleOffcanvas(\'offcanvas-responsive-menu-right\')"]');
        if (mobileMenuBtn) {
            mobileMenuBtn.removeAttribute('onclick');
            mobileMenuBtn.addEventListener('click', () => {
                const menu = document.getElementById('offcanvas-responsive-menu-right');
                const overlay = document.querySelector('.overlay');
                if (menu) {
                    menu.classList.remove('translate-x-full', 'rtl:-translate-x-full', 'invisible', 'opacity-0');
                    // Add both translation classes to handle both LTR and RTL correctly or just remove the hiding one
                    menu.style.transform = 'translateX(0)';
                    menu.classList.remove('invisible', 'opacity-0');
                }
                if (overlay) overlay.classList.remove('hidden');
            });
        }

        // Modal events
        const addressModal = document.getElementById('addressModal');
        const overlay = document.querySelector('#addressModal .fixed.inset-0.bg-gray-500'); // Modal overlay
        const cancelButton = document.querySelector('#addressModal button.bg-white'); // Cancel button

        // Close on overlay click
        if (addressModal) {
            addressModal.addEventListener('click', (e) => {
                if (e.target === addressModal || e.target.classList.contains('bg-gray-500')) {
                    this.closeModal();
                }
            });
        }

        if (cancelButton) {
            cancelButton.addEventListener('click', () => this.closeModal());
        }

        // Save button
        const saveButton = document.querySelector('#addressModal button.bg-primary');
        if (saveButton) {
            saveButton.addEventListener('click', () => this.saveAddress());
        }

        // Logout
        this.initLogout();
    },

    initLogout() {
        const logoutBtns = document.querySelectorAll('#logoutButton');
        logoutBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                window.authService.logout();
            });
        });
    },

    async checkAuthAndLoad() {
        if (!window.authService.isAuthenticated()) {
            window.location.href = 'login.html';
            return;
        }

        const user = await window.authService.getCurrentUser();
        if (user) {
            this.updateProfileInfo(user);
        }

        await this.loadAddresses();
    },

    updateProfileInfo(user) {
        // Update name in sidebar and header
        const nameElements = document.querySelectorAll('[data-user-name="true"]');
        const name = `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.email;
        nameElements.forEach(el => el.textContent = name);

        // Update avatar if we had one (using placeholder for now)
        // const avatarElements = document.querySelectorAll('img[alt="پروفایل کاربر"]');
        // avatarElements.forEach(el => el.src = user.avatarUrl || 'assets/images/user/user.png');
    },

    async loadAddresses() {
        try {
            const container = document.getElementById('addressesContainer');
            if (!container) return;

            window.utils.showLoading(container, 'در حال بارگذاری...');

            const response = await window.addressService.getAddresses();

            if (response.success && response.data) {
                this.renderAddresses(response.data);
            } else {
                window.utils.showToast(response.error || 'خطا در بارگذاری آدرس‌ها', 'error');
                container.innerHTML = `
                    <div class="col-span-1 md:col-span-2 text-center py-8 text-danger">
                        <p>${response.error || 'خطا در بارگذاری اطلاعات'}</p>
                        <button onclick="AddressManager.loadAddresses()" class="mt-4 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary/90">
                            تلاش مجدد
                        </button>
                    </div>
                `;
            }
        } catch (error) {
            console.error('Error loading addresses:', error);
            window.utils.showToast('خطا در اتصال به سرور', 'error');
        } finally {
            const container = document.getElementById('addressesContainer');
            if (container) window.utils.hideLoading(container);
        }
    },

    renderAddresses(addresses) {
        const container = document.getElementById('addressesContainer');
        if (!container) return;

        // Ensure array
        const list = Array.isArray(addresses) ? addresses : (addresses.data || []);

        if (list.length === 0) {
            container.innerHTML = `
                <div class="col-span-1 md:col-span-2 text-center py-12 text-gray-500 dark:text-gray-400 bg-white dark:bg-card-dark rounded-xl shadow-soft dark:shadow-soft-dark border border-gray-100 dark:border-gray-700">
                    <svg class="w-16 h-16 mx-auto mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"></path>
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    </svg>
                    <p class="text-lg mb-2">هنوز آدرسی ثبت نکرده‌اید</p>
                    <p class="text-sm text-gray-400 mb-6">برای ثبت سفارش، حداقل یک آدرس اضافه کنید</p>
                    <button onclick="AddressManager.openAddModal()" class="px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary/90 transition-colors shadow-lg shadow-primary/30">
                        افزودن آدرس جدید
                    </button>
                </div>
            `;
            return;
        }

        container.innerHTML = list.map(address => {
            const addressText = address.addressLine1 + (address.addressLine2 ? ` - ${address.addressLine2}` : '');
            const borderClass = address.isDefault ? 'border-2 border-primary dark:border-primary-dark' : 'border border-gray-200 dark:border-gray-700';

            return `
            <div class="bg-white dark:bg-card-dark rounded-xl shadow-soft dark:shadow-soft-dark p-6 ${borderClass} relative transition-all hover:shadow-lg">
                ${address.isDefault ? `
                    <div class="absolute top-4 end-4">
                        <span class="px-3 py-1 text-xs font-medium rounded-full bg-primary/10 text-primary dark:bg-primary-dark/20 dark:text-primary-400">پیش‌فرض</span>
                    </div>
                ` : ''}
                
                <div class="flex flex-col h-full">
                    <div class="flex-1 mb-4">
                        <div class="flex items-center mb-3">
                            <div class="p-2 rounded-lg bg-gray-100 dark:bg-gray-700 text-gray-500 dark:text-gray-400 me-3">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                                </svg>
                            </div>
                            <h3 class="text-lg font-bold text-dark dark:text-light">${address.title || 'آدرس'}</h3>
                        </div>
                        
                        <div class="space-y-2 text-sm text-gray-600 dark:text-gray-300">
                            <div class="flex items-start">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 me-2 text-gray-400 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM4 21a8 8 0 1116 0H4z" />
                                </svg>
                                <span>${address.firstName} ${address.lastName}</span>
                            </div>
                            
                            <div class="flex items-start">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 me-2 text-gray-400 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"></path>
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"></path>
                                </svg>
                                <div>
                                    <p>${address.state}، ${address.city}</p>
                                    <p class="mt-1 leading-relaxed">${addressText}</p>
                                </div>
                            </div>
                            
                            <div class="flex items-center">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 me-2 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                                </svg>
                                <span>کد پستی: ${address.postalCode}</span>
                            </div>
                            
                            ${address.phoneNumber ? `
                            <div class="flex items-center">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 me-2 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                                </svg>
                                <span>${address.phoneNumber}</span>
                            </div>
                            ` : ''}
                        </div>
                    </div>
                    
                    <div class="flex justify-between items-center pt-4 border-t border-gray-100 dark:border-gray-700 mt-auto">
                        ${!address.isDefault ? `
                            <button onclick="AddressManager.setDefault('${address.id}')" class="text-xs font-medium text-gray-500 hover:text-primary dark:text-gray-400 dark:hover:text-primary-400 transition-colors">
                                تنظیم به عنوان پیش‌فرض
                            </button>
                        ` : '<div></div>'}
                        
                        <div class="flex space-x-3 space-x-reverse">
                            <button onclick='AddressManager.openEditModal(${JSON.stringify(address)})' class="flex items-center text-sm font-medium text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 transition-colors">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 me-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                </svg>
                                ویرایش
                            </button>
                            <button onclick="AddressManager.deleteAddress('${address.id}')" class="flex items-center text-sm font-medium text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300 transition-colors">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 me-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                </svg>
                                حذف
                            </button>
                        </div>
                    </div>
                </div>
            </div>
            `;
        }).join('');
    },

    openAddModal() {
        this.editingAddressId = null;
        document.getElementById('modalTitle').textContent = 'افزودن آدرس جدید';
        this.resetForm();
        this.toggleModal(true);
    },

    openEditModal(address) {
        // If address is just an ID (passed as string), we might need to fetch it or find it in list
        // But the render passes the whole object JSON.stringify-ed
        if (typeof address === 'string') {
            // Fetch logic if needed, but we passed the object
            // For now assume it's the object
            console.error('Expected address object');
            return;
        }

        this.editingAddressId = address.id;
        document.getElementById('modalTitle').textContent = 'ویرایش آدرس';

        // Populate form
        document.getElementById('addressTitle').value = address.title || '';
        document.getElementById('province').value = address.state || '';
        document.getElementById('city').value = address.city || '';
        document.getElementById('address').value = address.addressLine1 || '';
        document.getElementById('postalCode').value = address.postalCode || '';
        document.getElementById('receiverName').value = `${address.firstName || ''} ${address.lastName || ''}`.trim();
        document.getElementById('phone').value = address.phoneNumber || '';
        document.getElementById('defaultAddress').checked = address.isDefault || false;

        this.toggleModal(true);
    },

    closeModal() {
        this.toggleModal(false);
        this.editingAddressId = null;
        this.resetForm();
    },

    toggleModal(show) {
        const modal = document.getElementById('addressModal');
        if (show) {
            modal.classList.remove('hidden');
        } else {
            modal.classList.add('hidden');
        }
    },

    resetForm() {
        document.querySelector('#addressModal form').reset();
        // Clear errors
        document.querySelectorAll('.error-message').forEach(el => el.remove());
        document.querySelectorAll('.input-error').forEach(el => el.classList.remove('input-error'));
    },

    async saveAddress() {
        const title = document.getElementById('addressTitle')?.value || '';
        const province = document.getElementById('province')?.value || '';
        const city = document.getElementById('city')?.value || '';
        const address = document.getElementById('address')?.value || '';
        const postalCode = document.getElementById('postalCode')?.value || '';
        const receiverName = document.getElementById('receiverName')?.value || '';
        const phone = document.getElementById('phone')?.value || '';
        const isDefault = document.getElementById('defaultAddress')?.checked || false;

        // Split receiver name
        const nameParts = receiverName.trim().split(' ');
        const firstName = nameParts[0] || '';
        const lastName = nameParts.slice(1).join(' ') || '';

        const addressData = {
            title: title,
            firstName: firstName,
            lastName: lastName,
            addressLine1: address,
            city: city,
            state: province,
            postalCode: postalCode,
            phoneNumber: phone,
            country: 'Iran',
            isDefault: isDefault,
            isShippingAddress: true,
            isBillingAddress: isDefault
        };

        // Validate
        const validation = window.addressService.validateAddressData(addressData);
        if (!validation.isValid) {
            this.displayValidationErrors(validation.errors);
            return;
        }

        try {
            const btn = document.querySelector('#addressModal button.bg-primary');
            const originalText = btn.textContent;
            btn.textContent = 'در حال ذخیره...';
            btn.disabled = true;

            let response;
            if (this.editingAddressId) {
                response = await window.addressService.updateAddress(this.editingAddressId, addressData);
            } else {
                response = await window.addressService.createAddress(addressData);
            }

            if (response.success) {
                window.utils.showToast(this.editingAddressId ? 'آدرس با موفقیت ویرایش شد' : 'آدرس جدید با موفقیت اضافه شد', 'success');
                this.closeModal();
                this.loadAddresses();
            } else {
                window.utils.showToast(response.error || 'خطا در ذخیره آدرس', 'error');
            }

            btn.textContent = originalText;
            btn.disabled = false;
        } catch (error) {
            console.error('Error saving address:', error);
            window.utils.showToast('خطا در اتصال به سرور', 'error');

            const btn = document.querySelector('#addressModal button.bg-primary');
            btn.textContent = 'ذخیره آدرس';
            btn.disabled = false;
        }
    },

    displayValidationErrors(errors) {
        // Clear previous errors
        document.querySelectorAll('.error-message').forEach(el => el.remove());
        document.querySelectorAll('.input-error').forEach(el => el.classList.remove('input-error'));

        // Element ID mapping (addressData key -> HTML ID)
        const fieldMap = {
            title: 'addressTitle',
            state: 'province',
            city: 'city',
            addressLine1: 'address',
            postalCode: 'postalCode',
            firstName: 'receiverName', // Map both to receiverName
            phoneNumber: 'phone'
        };

        // Display new errors
        Object.keys(errors).forEach(field => {
            const elementId = fieldMap[field];
            if (!elementId) return;

            const input = document.getElementById(elementId);
            if (input) {
                input.classList.add('input-error', 'border-red-500');

                // Don't add duplicate error for name
                if (field === 'lastName' && errors.firstName) return;

                // Check if error already exists
                if (!input.parentNode.querySelector('.error-message')) {
                    const errorDiv = document.createElement('div');
                    errorDiv.className = 'error-message text-red-500 text-xs mt-1';
                    errorDiv.textContent = errors[field];
                    input.parentNode.appendChild(errorDiv);
                }
            }
        });
    },

    async deleteAddress(id) {
        if (!await window.utils.confirmAction('آیا از حذف این آدرس مطمئن هستید؟')) {
            return;
        }

        try {
            window.utils.showLoading(document.getElementById('addressesContainer'), 'در حال حذف...');
            const response = await window.addressService.deleteAddress(id);

            if (response.success) {
                window.utils.showToast('آدرس با موفقیت حذف شد', 'success');
                this.loadAddresses();
            } else {
                window.utils.showToast(response.error || 'خطا در حذف آدرس', 'error');
                window.utils.hideLoading(document.getElementById('addressesContainer'));
            }
        } catch (error) {
            console.error('Delete error:', error);
            window.utils.showToast('خطا در اتصال به سرور', 'error');
            window.utils.hideLoading(document.getElementById('addressesContainer'));
        }
    },

    async setDefault(id) {
        try {
            const container = document.getElementById('addressesContainer');
            // Optimistic update or show loading
            window.utils.showLoading(container, 'در حال تنظیم پیش‌فرض...');

            const response = await window.addressService.setDefaultAddress(id);

            if (response.success) {
                window.utils.showToast('آدرس پیش‌فرض تغییر کرد', 'success');
                this.loadAddresses();
            } else {
                window.utils.showToast(response.error || 'خطا در تنظیم آدرس', 'error');
                window.utils.hideLoading(container);
            }
        } catch (error) {
            console.error('Set default error:', error);
            window.utils.showToast('خطا در اتصال به سرور', 'error');
            window.utils.hideLoading(document.getElementById('addressesContainer'));
        }
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    AddressManager.init();
    // Expose for onclick events
    window.AddressManager = AddressManager;
});
