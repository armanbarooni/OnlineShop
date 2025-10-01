using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.Unit
{
    public class UnitDetailsDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string Comment { get; init; } = null!;
        public bool Deleted { get; init; }
        public long RowVersion { get; init; }
    }
}