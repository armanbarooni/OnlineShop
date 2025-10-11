using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class UserAddress : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string AddressLine1 { get; private set; } = string.Empty;
        public string? AddressLine2 { get; private set; }
        public string City { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty;
        public string PostalCode { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;
        public string? PhoneNumber { get; private set; }
        public bool IsDefault { get; private set; }
        public bool IsBillingAddress { get; private set; }
        public bool IsShippingAddress { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;

        protected UserAddress() { }

        private UserAddress(Guid userId, string title, string firstName, string lastName, 
            string addressLine1, string city, string state, string postalCode, string country)
        {
            UserId = userId;
            SetTitle(title);
            SetFirstName(firstName);
            SetLastName(lastName);
            SetAddressLine1(addressLine1);
            SetCity(city);
            SetState(state);
            SetPostalCode(postalCode);
            SetCountry(country);
            IsDefault = false;
            IsBillingAddress = false;
            IsShippingAddress = false;
            Deleted = false;
        }

        public static UserAddress Create(Guid userId, string title, string firstName, string lastName, 
            string addressLine1, string city, string state, string postalCode, string country)
            => new(userId, title, firstName, lastName, addressLine1, city, state, postalCode, country);

        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("عنوان آدرس نباید خالی باشد");
            Title = title.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("نام نباید خالی باشد");
            FirstName = firstName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("نام خانوادگی نباید خالی باشد");
            LastName = lastName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAddressLine1(string addressLine1)
        {
            if (string.IsNullOrWhiteSpace(addressLine1))
                throw new ArgumentException("آدرس نباید خالی باشد");
            AddressLine1 = addressLine1.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAddressLine2(string? addressLine2)
        {
            AddressLine2 = addressLine2?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("شهر نباید خالی باشد");
            City = city.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("استان نباید خالی باشد");
            State = state.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPostalCode(string postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("کد پستی نباید خالی باشد");
            PostalCode = postalCode.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCountry(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("کشور نباید خالی باشد");
            Country = country.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPhoneNumber(string? phoneNumber)
        {
            PhoneNumber = phoneNumber?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsDefault()
        {
            IsDefault = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveAsDefault()
        {
            IsDefault = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsBillingAddress(bool isBilling)
        {
            IsBillingAddress = isBilling;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsShippingAddress(bool isShipping)
        {
            IsShippingAddress = isShipping;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string title, string firstName, string lastName, string addressLine1, 
            string? addressLine2, string city, string state, string postalCode, string country, 
            string? phoneNumber, bool isDefault, bool isBillingAddress, bool isShippingAddress, string? updatedBy)
        {
            SetTitle(title);
            SetFirstName(firstName);
            SetLastName(lastName);
            SetAddressLine1(addressLine1);
            SetAddressLine2(addressLine2);
            SetCity(city);
            SetState(state);
            SetPostalCode(postalCode);
            SetCountry(country);
            SetPhoneNumber(phoneNumber);
            
            if (isDefault) SetAsDefault();
            else RemoveAsDefault();
            
            SetAsBillingAddress(isBillingAddress);
            SetAsShippingAddress(isShippingAddress);
            
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این آدرس قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
