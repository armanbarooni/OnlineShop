namespace OnlineShop.Infrastructure.Mahak.Models
{
    /// <summary>
    /// Model for Person (Customer) data from/to Mahak
    /// </summary>
    public class PersonModel
    {
        public int PersonId { get; set; }
        public long PersonClientId { get; set; }
        public int PersonCode { get; set; }
        public int? PersonGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? NationalCode { get; set; }
        public string? EconomicCode { get; set; }
        public string? PostalCode { get; set; }
        public string? Address { get; set; }
        public int Type { get; set; } // 0=Real person, 1=Legal entity
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
    }

    /// <summary>
    /// Model for Person Address from/to Mahak
    /// </summary>
    public class PersonAddressModel
    {
        public int PersonAddressId { get; set; }
        public long PersonAddressClientId { get; set; }
        public int PersonId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
    }

    /// <summary>
    /// Model for sending Person to Mahak
    /// </summary>
    public class MahakPersonModel
    {
        public long PersonClientId { get; set; }
        public int? PersonGroupId { get; set; } // Required for creating person
        public string Name { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? NationalCode { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public int Type { get; set; } = 0; // Real person by default
        public bool Deleted { get; set; } = false;
    }

    /// <summary>
    /// Model for sending VisitorPeople (Person-Visitor link) to Mahak
    /// Required for real sales with inventory deduction
    /// </summary>
    public class MahakVisitorPersonModel
    {
        public long VisitorPersonClientId { get; set; }
        public int VisitorId { get; set; }
        public long PersonClientId { get; set; }
        public bool Deleted { get; set; } = false;
    }

    /// <summary>
    /// Model for sending Person Address to Mahak
    /// </summary>
    public class MahakPersonAddressModel
    {
        public long PersonAddressClientId { get; set; }
        public long PersonClientId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
