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
        public string Name { get; init; } = default!;
        public string Comment { get; init; } = default!;
        public bool Deleted { get; init; }
        public long RowVersion { get; init; }
    }
}