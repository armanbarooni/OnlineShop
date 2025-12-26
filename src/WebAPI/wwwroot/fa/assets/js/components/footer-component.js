/**
 * Footer Component
 * Manages common footer functionality across all pages
 */

class FooterComponent {
    constructor() {
        this.init();
    }

    async init() {
        this.setupEventListeners();
        await this.loadNewsletterSubscription();
    }

    setupEventListeners() {
        // Newsletter subscription
        const newsletterForm = document.getElementById('newsletterForm');
        if (newsletterForm) {
            newsletterForm.addEventListener('submit', (e) => {
                this.handleNewsletterSubmit(e);
            });
        }

        // Back to top button
        const backToTopBtn = document.querySelector('[href="#top"]');
        if (backToTopBtn) {
            backToTopBtn.addEventListener('click', (e) => {
                e.preventDefault();
                window.scrollTo({
                    top: 0,
                    behavior: 'smooth'
                });
            });
        }

        // Show/hide back to top button on scroll
        window.addEventListener('scroll', () => {
            const backToTopBtn = document.querySelector('[href="#top"]');
            if (backToTopBtn) {
                if (window.scrollY > 300) {
                    backToTopBtn.style.opacity = '1';
                    backToTopBtn.style.visibility = 'visible';
                } else {
                    backToTopBtn.style.opacity = '0';
                    backToTopBtn.style.visibility = 'hidden';
                }
            }
        });
    }

    async handleNewsletterSubmit(e) {
        e.preventDefault();
        
        const emailInput = e.target.querySelector('input[type="email"]');
        if (!emailInput) return;

        const email = emailInput.value.trim();
        if (!email) {
            if (window.utils) {
                window.utils.showToast('لطفاً ایمیل خود را وارد کنید.', 'error');
            }
            return;
        }

        // Email validation
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            if (window.utils) {
                window.utils.showToast('ایمیل وارد شده معتبر نیست.', 'error');
            }
            return;
        }

        try {
            // Here you can add API call to subscribe to newsletter
            // const response = await window.apiClient.post('/api/Newsletter/Subscribe', { email });
            
            // For now, just show success message
            if (window.utils) {
                window.utils.showToast('اشتراک خبرنامه با موفقیت انجام شد.', 'success');
            }
            emailInput.value = '';
        } catch (error) {
            window.logger.error('Error subscribing to newsletter:', error);
            if (window.utils) {
                window.utils.showToast('خطایی در ثبت‌نام خبرنامه رخ داد.', 'error');
            }
        }
    }

    async loadNewsletterSubscription() {
        // If there's a newsletter subscription form, we can pre-populate it with user email if logged in
        if (window.authService && window.authService.isAuthenticated()) {
            try {
                const user = await window.authService.getCurrentUser();
                if (user && user.email) {
                    const emailInput = document.querySelector('#newsletterForm input[type="email"]');
                    if (emailInput && !emailInput.value) {
                        emailInput.value = user.email;
                    }
                }
            } catch (error) {
                window.logger.error('Error loading newsletter subscription:', error);
            }
        }
    }

    // Method to update footer links dynamically if needed
    updateFooterLinks(links) {
        const footerLinksContainer = document.getElementById('footerLinks');
        if (!footerLinksContainer || !links) return;

        const html = links.map(link => `
            <li>
                <a href="${link.url}" class="text-gray-600 hover:text-primary transition-colors">
                    ${link.title}
                </a>
            </li>
        `).join('');

        footerLinksContainer.innerHTML = html;
    }

    // Method to update social media links
    updateSocialLinks(socialLinks) {
        const socialLinksContainer = document.getElementById('socialLinks');
        if (!socialLinksContainer || !socialLinks) return;

        const html = socialLinks.map(link => `
            <a href="${link.url}" target="_blank" rel="noopener noreferrer" 
               class="w-10 h-10 flex items-center justify-center rounded-full bg-gray-100 hover:bg-primary hover:text-white transition-colors">
                <i class="${link.icon}"></i>
            </a>
        `).join('');

        socialLinksContainer.innerHTML = html;
    }

    // Method to update contact information
    updateContactInfo(contactInfo) {
        if (!contactInfo) return;

        // Update phone
        const phoneEl = document.getElementById('footerPhone');
        if (phoneEl && contactInfo.phone) {
            phoneEl.textContent = contactInfo.phone;
            phoneEl.href = `tel:${contactInfo.phone}`;
        }

        // Update email
        const emailEl = document.getElementById('footerEmail');
        if (emailEl && contactInfo.email) {
            emailEl.textContent = contactInfo.email;
            emailEl.href = `mailto:${contactInfo.email}`;
        }

        // Update address
        const addressEl = document.getElementById('footerAddress');
        if (addressEl && contactInfo.address) {
            addressEl.textContent = contactInfo.address;
        }
    }
}

// Initialize footer component when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (typeof window.FooterComponent === 'undefined') {
        window.footerComponent = new FooterComponent();
    }
});



