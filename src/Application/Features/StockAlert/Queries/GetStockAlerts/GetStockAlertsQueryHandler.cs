using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.StockAlert;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.StockAlert.Queries.GetStockAlerts
{
    public class GetStockAlertsQueryHandler : IRequestHandler<GetStockAlertsQuery, Result<List<StockAlertDto>>>
    {
        private readonly IStockAlertRepository _stockAlertRepository;
        private readonly IMapper _mapper;

        public GetStockAlertsQueryHandler(IStockAlertRepository stockAlertRepository, IMapper mapper)
        {
            _stockAlertRepository = stockAlertRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<StockAlertDto>>> Handle(GetStockAlertsQuery request, CancellationToken cancellationToken)
        {
            var alerts = new List<Domain.Entities.StockAlert>();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                alerts = await _stockAlertRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            }
            else if (request.ProductId.HasValue)
            {
                alerts = await _stockAlertRepository.GetByProductIdAsync(request.ProductId.Value, cancellationToken);
            }
            else
            {
                // Get all alerts with pagination
                var allAlerts = await _stockAlertRepository.GetAllAsync(cancellationToken);
                alerts = allAlerts.ToList();
            }

            // Apply filters
            if (request.Notified.HasValue)
            {
                alerts = alerts.Where(a => a.Notified == request.Notified.Value).ToList();
            }

            // Apply pagination
            var pagedAlerts = alerts
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var alertDtos = _mapper.Map<List<StockAlertDto>>(pagedAlerts);

            return Result<List<StockAlertDto>>.Success(alertDtos);
        }
    }
}
