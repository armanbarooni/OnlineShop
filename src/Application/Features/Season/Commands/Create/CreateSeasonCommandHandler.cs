using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Season;
using OnlineShop.Domain.Entities;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Season.Commands.Create
{
    public class CreateSeasonCommandHandler : IRequestHandler<CreateSeasonCommand, Result<SeasonDto>>
    {
        private readonly ISeasonRepository _repository;
        private readonly IMapper _mapper;

        public CreateSeasonCommandHandler(ISeasonRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<SeasonDto>> Handle(CreateSeasonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if season with same name already exists
                var existingSeason = await _repository.ExistsByNameAsync(request.Season.Name, cancellationToken);
                if (existingSeason)
                {
                    return Result<SeasonDto>.Failure("فصل با این نام قبلاً وجود دارد");
                }

                // Create new season (auto-generate code if not provided)
                var code = string.IsNullOrWhiteSpace(request.Season.Code)
                    ? new string((request.Season.Name ?? string.Empty)
                        .Trim()
                        .ToUpperInvariant()
                        .Replace(' ', '-')
                        .Where(char.IsLetterOrDigit)
                        .ToArray())
                    : request.Season.Code;

                if (string.IsNullOrWhiteSpace(code))
                {
                    code = $"SEASON-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
                }

                var season = Domain.Entities.Season.Create(request.Season.Name, code);
                
                await _repository.AddAsync(season, cancellationToken);

                var seasonDto = _mapper.Map<SeasonDto>(season);
                return Result<SeasonDto>.Success(seasonDto);
            }
            catch (Exception ex)
            {
                return Result<SeasonDto>.Failure($"خطا در ایجاد فصل: {ex.Message}");
            }
        }
    }
}


