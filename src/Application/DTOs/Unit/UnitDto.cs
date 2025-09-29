using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.Unit
{
    public class UnitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Comment { get; set; } = default!;
    }
}