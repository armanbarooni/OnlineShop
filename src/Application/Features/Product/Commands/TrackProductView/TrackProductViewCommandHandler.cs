using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Features.Product.Commands.TrackProductView;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Product.Commands.TrackProductView
{
    public class TrackProductViewCommandHandler : IRequestHandler<TrackProductViewCommand, Result<bool>>
    {
        private readonly IUserProductViewRepository _userProductViewRepository;
        private readonly IProductRepository _productRepository;

        public TrackProductViewCommandHandler(
            IUserProductViewRepository userProductViewRepository,
            IProductRepository productRepository)
        {
            _userProductViewRepository = userProductViewRepository;
            _productRepository = productRepository;
        }

        public async Task<Result<bool>> Handle(TrackProductViewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if product exists
                var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
                if (product == null)
                {
                    return Result<bool>.Failure("محصول مورد نظر یافت نشد");
                }

                // Check if user already viewed this product recently (within last hour)
                var recentViews = await _userProductViewRepository.GetByUserIdAsync(Guid.Parse(request.UserId), 50, cancellationToken);
                var recentView = recentViews
                    .Where(rv => rv.ProductId == request.ProductId)
                    .OrderByDescending(rv => rv.ViewedAt)
                    .FirstOrDefault();

                if (recentView != null && recentView.ViewedAt > DateTime.UtcNow.AddHours(-1))
                {
                    // Update existing view time
                    recentView.UpdateViewTime();
                    await _userProductViewRepository.UpdateAsync(recentView, cancellationToken);
                }
                else
                {
                    // Create new view record
                    var userProductView = UserProductView.Create(
                        Guid.Parse(request.UserId),
                        request.ProductId,
                        request.SessionId,
                        request.UserAgent,
                        request.IpAddress);

                    await _userProductViewRepository.AddAsync(userProductView, cancellationToken);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در ثبت بازدید محصول: {ex.Message}");
            }
        }
    }
}
