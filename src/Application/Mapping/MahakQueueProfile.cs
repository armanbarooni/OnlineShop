using AutoMapper;
using OnlineShop.Application.DTOs.MahakQueue;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class MahakQueueProfile : Profile
    {
        public MahakQueueProfile()
        {
            CreateMap<MahakQueue, MahakQueueDto>();

            CreateMap<CreateMahakQueueDto, MahakQueue>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.QueueStatus, opt => opt.Ignore())
                .ForMember(d => d.RetryCount, opt => opt.Ignore())
                .ForMember(d => d.ProcessedAt, opt => opt.Ignore())
                .ForMember(d => d.FailedAt, opt => opt.Ignore())
                .ForMember(d => d.ErrorMessage, opt => opt.Ignore())
                .ForMember(d => d.MahakResponse, opt => opt.Ignore())
                .ForMember(d => d.NextRetryAt, opt => opt.Ignore())
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

            CreateMap<UpdateMahakQueueDto, MahakQueue>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.EntityId, opt => opt.Ignore())
                .ForMember(d => d.QueueStatus, opt => opt.Ignore())
                .ForMember(d => d.RetryCount, opt => opt.Ignore())
                .ForMember(d => d.MaxRetries, opt => opt.Ignore())
                .ForMember(d => d.ScheduledAt, opt => opt.Ignore())
                .ForMember(d => d.ProcessedAt, opt => opt.Ignore())
                .ForMember(d => d.FailedAt, opt => opt.Ignore())
                .ForMember(d => d.ErrorMessage, opt => opt.Ignore())
                .ForMember(d => d.MahakResponse, opt => opt.Ignore())
                .ForMember(d => d.NextRetryAt, opt => opt.Ignore())
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

