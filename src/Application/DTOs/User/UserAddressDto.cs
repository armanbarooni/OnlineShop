namespace OnlineShop.Application.DTOs.User
{
    public class UserAddressDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Provincial { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class CreateAddressDto
    {
        public string Title { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Provincial { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }

    public class UpdateAddressDto : CreateAddressDto
    {
        public Guid Id { get; set; }
    }
}
