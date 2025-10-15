using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Features.Season.Commands.Update
{
    public class UpdateSeasonCommandHandler : IRequestHandler<UpdateSeasonCommand, Result<SeasonDto>>
    {
        private readonly ISeasonRepository _repository;
        private readonly IMapper _mapper;

        public UpdateSeasonCommandHandler(ISeasonRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<SeasonDto>> Handle(UpdateSeasonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var season = await _repository.GetByIdAsync(request.Season.Id, cancellationToken);
                if (season == null)
                {
                    return Result<SeasonDto>.Failure("فصل مورد نظر یافت نشد");
                }

                // Check if another season with same name exists (excluding current one)
                var existingSeason = await _repository.ExistsByNameAsync(request.Season.Name, cancellationToken);
                if (existingSeason)
                {
                    // Get the existing season to check if it's the same one
                    var allSeasons = await _repository.GetAllAsync(cancellationToken);
                    var seasonWithSameName = allSeasons.FirstOrDefault(s => s.Name == request.Season.Name && s.Id != request.Season.Id);
                    if (seasonWithSameName != null)
                    {
                        return Result<SeasonDto>.Failure("فصل با این نام قبلاً وجود دارد");
                    }
                }

                // Update season
                season.Update(request.Season.Name, request.Season.Code, "System");
                
                await _repository.UpdateAsync(season, cancellationToken);

                var seasonDto = _mapper.Map<SeasonDto>(season);
                return Result<SeasonDto>.Success(seasonDto);
            }
            catch (Exception ex)
            {
                return Result<SeasonDto>.Failure($"خطا در به‌روزرسانی فصل: {ex.Message}");
            }
        }
    }
}
