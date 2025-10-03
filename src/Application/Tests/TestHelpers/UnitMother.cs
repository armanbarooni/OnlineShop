// TestHelpers/UnitMother.cs
using System.Reflection;
using OnlineShop.Domain.Entities;


namespace OnlineShop.Application.Tests.TestHelpers
{
    public static class UnitMother
    {
        public static Unit CreateWithId(
            Guid id,
            string name = "Test Unit",
            string comment = "Test Comment",
            int unitCode = 1,
            long mahakClientId = 123,
            int mahakId = 1)
        {
            var unit = Unit.Create(unitCode, name, mahakClientId, mahakId, comment);

            // Set ID using reflection
            var idProperty = typeof(Unit).GetProperty("Id");
            idProperty?.SetValue(unit, id);

            return unit;
        }

        public static Unit CreateValid(
            string name = "Kilogram",
            string comment = "Weight unit",
            int unitCode = 1,
            long mahakClientId = 123,
            int mahakId = 1)
        {
            return Unit.Create(unitCode, name, mahakClientId, mahakId, comment);
        }
    }
}
