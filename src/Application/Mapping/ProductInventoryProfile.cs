using AutoMapper;
using OnlineShop.Application.DTOs.ProductInventory;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductInventoryProfile : Profile
    {
        public ProductInventoryProfile()
        {
            CreateMap<ProductInventory, ProductInventoryDto>()
                .ForMember(d => d.UnitId, opt => opt.Ignore())
                .ForMember(d => d.Quantity, opt => opt.Ignore())
                .ForMember(d => d.UnitPrice, opt => opt.Ignore())
                .ForMember(d => d.Location, opt => opt.Ignore())
                .ForMember(d => d.LastUpdated, opt => opt.Ignore());
            
            CreateMap<CreateProductInventoryDto, ProductInventory>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.LastSyncAt, opt => opt.Ignore())
                .ForMember(d => d.SyncStatus, opt => opt.Ignore())
                .ForMember(d => d.SyncError, opt => opt.Ignore())
                .ForMember(d => d.Product, opt => opt.Ignore())
                .ForMember(d => d.MahakId, opt => opt.Ignore())
                .ForMember(d => d.MahakClientId, opt => opt.Ignore())
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ForMember(d => d.Deleted, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedAt, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedBy, opt => opt.Ignore());
            
            CreateMap<UpdateProductInventoryDto, ProductInventory>()
                .ForMember(d => d.ProductId, opt => opt.Ignore())
                .ForMember(d => d.LastSyncAt, opt => opt.Ignore())
                .ForMember(d => d.SyncStatus, opt => opt.Ignore())
                .ForMember(d => d.SyncError, opt => opt.Ignore())
                .ForMember(d => d.Product, opt => opt.Ignore())
                .ForMember(d => d.MahakId, opt => opt.Ignore())
                .ForMember(d => d.MahakClientId, opt => opt.Ignore())
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ForMember(d => d.Deleted, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedAt, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedBy, opt => opt.Ignore());
        }
    }
}
