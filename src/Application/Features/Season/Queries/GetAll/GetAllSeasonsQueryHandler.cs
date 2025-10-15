using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Features.Season.Queries.GetAll
{
    public class GetAllSeasonsQueryHandler : IRequestHandler<GetAllSeasonsQuery, Result<List<SeasonDto>>>
    {
        private readonly ISeasonRepository _repository;
        private readonly IMapper _mapper;

        public GetAllSeasonsQueryHandler(ISeasonRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<SeasonDto>>> Handle(GetAllSeasonsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var seasons = await _repository.GetAllAsync(cancellationToken);
                var seasonDtos = _mapper.Map<List<SeasonDto>>(seasons);
                return Result<List<SeasonDto>>.Success(seasonDtos);
            }
            catch (Exception ex)
            {
                return Result<List<SeasonDto>>.Failure($"خطا در دریافت لیست فصل‌ها: {ex.Message}");
            }
        }
    }
}
