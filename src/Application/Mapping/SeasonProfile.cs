using AutoMapper;
using OnlineShop.Application.DTOs.Season;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class SeasonProfile : Profile
    {
        public SeasonProfile()
        {
            CreateMap<Season, SeasonDto>();
            CreateMap<CreateSeasonDto, Season>();
            CreateMap<UpdateSeasonDto, Season>();
        }
    }
}

