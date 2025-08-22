using OnlineShop.Domain.Common;

public class Unit : BaseEntity
{
    public int UnitCode { get; private set; }
    public string Name { get; private set; }
    public string Comment { get; private set; }
    public string UnitTIN { get; private set; }

    protected Unit() { }

    private Unit(int unitCode, string name, string unitTIN, long mahakClientId, int mahakId, string comment)
    {
        SetName(name);
        SetComment(comment);
        UnitCode = unitCode;
        UnitTIN = unitTIN?.Trim();
        MahakClientId = mahakClientId;
        MahakId = mahakId;
        Deleted = false;
    }

    public static Unit Create(int unitCode, string name, string unitTIN, long mahakClientId, int mahakId, string comment)
        => new(unitCode, name, unitTIN, mahakClientId, mahakId, comment);

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام واحد نباید خالی باشد");
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetComment(string comment)
    {
        Comment = comment?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (Deleted) return;
        Deleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
