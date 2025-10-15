using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Unit.Command.Update
{
    public class UpdateUnitCommand:IRequest<Result<bool>>
    {
        public UpdateUnitDto? UnitDto { get; set; }
    }
}

