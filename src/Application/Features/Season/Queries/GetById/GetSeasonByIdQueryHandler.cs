using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Season;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Season.Queries.GetById
{
    public class GetSeasonByIdQueryHandler : IRequestHandler<GetSeasonByIdQuery, Result<SeasonDto>>
    {
        private readonly ISeasonRepository _repository;
        private readonly IMapper _mapper;

        public GetSeasonByIdQueryHandler(ISeasonRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<SeasonDto>> Handle(GetSeasonByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var season = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (season == null)
                {
                    return Result<SeasonDto>.Failure("فصل مورد نظر یافت نشد");
                }

                var seasonDto = _mapper.Map<SeasonDto>(season);
                return Result<SeasonDto>.Success(seasonDto);
            }
            catch (Exception ex)
            {
                return Result<SeasonDto>.Failure($"خطا در دریافت فصل: {ex.Message}");
            }
        }
    }
}


