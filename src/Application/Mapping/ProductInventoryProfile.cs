using AutoMapper;
using OnlineShop.Application.DTOs.ProductInventory;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductInventoryProfile : Profile
    {
        public ProductInventoryProfile()
        {
            CreateMap<ProductInventory, ProductInventoryDto>();
            CreateMap<CreateProductInventoryDto, ProductInventory>();
            CreateMap<UpdateProductInventoryDto, ProductInventory>();
        }
    }
}
