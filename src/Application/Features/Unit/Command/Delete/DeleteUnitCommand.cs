using MediatR;
using OnlineShop.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Unit.Command.Delete
{
    public class DeleteUnitCommand:IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}
