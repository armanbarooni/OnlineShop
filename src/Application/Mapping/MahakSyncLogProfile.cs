using AutoMapper;
using OnlineShop.Application.DTOs.MahakSyncLog;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class MahakSyncLogProfile : Profile
    {
        public MahakSyncLogProfile()
        {
            CreateMap<MahakSyncLog, MahakSyncLogDto>();

            CreateMap<CreateMahakSyncLogDto, MahakSyncLog>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.SyncStartedAt, opt => opt.Ignore())
                .ForMember(d => d.SyncCompletedAt, opt => opt.Ignore())
                .ForMember(d => d.DurationMs, opt => opt.Ignore())
                .ForMember(d => d.RecordsSuccessful, opt => opt.Ignore())
                .ForMember(d => d.RecordsFailed, opt => opt.Ignore())
                .ForMember(d => d.ErrorMessage, opt => opt.Ignore())
                .ForMember(d => d.SyncData, opt => opt.Ignore())
                .ForMember(d => d.MahakResponse, opt => opt.Ignore())
                .ForMember(d => d.MahakRowVersion, opt => opt.Ignore())
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

            CreateMap<UpdateMahakSyncLogDto, MahakSyncLog>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.EntityId, opt => opt.Ignore())
                .ForMember(d => d.SyncStartedAt, opt => opt.Ignore())
                .ForMember(d => d.SyncCompletedAt, opt => opt.Ignore())
                .ForMember(d => d.DurationMs, opt => opt.Ignore())
                .ForMember(d => d.RecordsSuccessful, opt => opt.Ignore())
                .ForMember(d => d.RecordsFailed, opt => opt.Ignore())
                .ForMember(d => d.ErrorMessage, opt => opt.Ignore())
                .ForMember(d => d.SyncData, opt => opt.Ignore())
                .ForMember(d => d.MahakResponse, opt => opt.Ignore())
                .ForMember(d => d.MahakRowVersion, opt => opt.Ignore())
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

