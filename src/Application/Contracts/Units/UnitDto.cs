using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.Contracts.Units
{
    public record UnitDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
        public string Symbol { get; init; } = default!;
    }
}
