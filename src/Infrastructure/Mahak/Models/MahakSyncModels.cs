using System;
using System.Collections.Generic;

namespace OnlineShop.Infrastructure.Mahak.Models
{
    public class RequestAllDataModel
    {
        public long FromProductVersion { get; set; }
        public long FromProductDetailVersion { get; set; }
        public long FromProductCategoryVersion { get; set; }
        public long FromPictureVersion { get; set; }
        public long FromPhotoGalleryVersion { get; set; }
        public long FromProductDetailStoreAssetVersion { get; set; }
        public long FromPersonVersion { get; set; }
        public long FromPersonAddressVersion { get; set; }
        public long FromVisitorPersonVersion { get; set; }
        public long FromRegionVersion { get; set; }
        // Add other Versions as needed
    }

    public class MahakApiResult<T>
    {
        public bool Result { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }
    }

    public class SaveAllDataResultApiResult
    {
        public bool Result { get; set; }
        public int Code { get; set; }
        public string? Message { get; set; }
        public SaveAllDataResultObject? Data { get; set; }
    }

    public class SaveAllDataResultObject
    {
        public SaveAllDataResult? Objects { get; set; }
    }

    public class SaveAllDataResult
    {
        public MultiEntityUpdateResult? People { get; set; }
        public MultiEntityUpdateResult? VisitorPeople { get; set; }
        public MultiEntityUpdateResult? Orders { get; set; }
        public MultiEntityUpdateResult? OrderDetails { get; set; }
    }

    public class MultiEntityUpdateResult
    {
        public List<EntityUpdateResult>? Results { get; set; }
    }

    public class EntityUpdateResult
    {
        public bool Result { get; set; }
        public int Index { get; set; }
        public int EntityId { get; set; }
        public long EntityClientId { get; set; }
        public int EntityCode { get; set; }
        public long RowVersion { get; set; }
        public List<PropertyErrorModel>? Errors { get; set; }
    }

    public class PropertyErrorModel
    {
        public string? Property { get; set; }
        public int Code { get; set; }
        public string? Error { get; set; }
    }

    public class GetAllDataResponse
    {
        public CommitDataModel Objects { get; set; }
    }

    public class CommitDataModel
    {
        public List<ProductModel>? Products { get; set; }
        public List<ProductDetailModel>? ProductDetails { get; set; }
        public List<ProductDetailStoreAssetModel>? ProductDetailStoreAssets { get; set; }
        public List<ProductCategoryModel>? ProductCategories { get; set; }
        public List<PictureModel>? Pictures { get; set; }
        public List<PhotoGalleryModel>? PhotoGalleries { get; set; }
        public List<PersonModel>? People { get; set; }
        public List<PersonAddressModel>? PersonAddresses { get; set; }
        public List<VisitorPersonModel>? VisitorPeople { get; set; }
        public List<RegionModel>? Regions { get; set; }
        // Add other lists as needed
    }

    public class RegionModel
    {
        public int CityID { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public string? MapCode { get; set; }
        public long RowVersion { get; set; }
    }

    public class VisitorPersonModel
    {
        public int VisitorPersonId { get; set; }
        public int PersonId { get; set; }
        public int VisitorId { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
        public long PersonClientId { get; set; }
        public int PersonCode { get; set; }
        public long VisitorClientId { get; set; }
        public int VisitorCode { get; set; }
    }
    
    public class LoginModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public long DatabaseId { get; set; }
        public string PackageNo { get; set; } = string.Empty;
        public string Language { get; set; } = "fa";
        public string AppId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientVersion { get; set; } = string.Empty;
    }

    public class LoginResultModel
    {
        public string UserToken { get; set; } = string.Empty;
        public long SyncId { get; set; }
        public long VisitorId { get; set; }
        public long DatabaseId { get; set; }
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserTitle { get; set; }
        public string? MahakId { get; set; }
        public DateTime ServerTime { get; set; }
        public long PackageNo { get; set; }
        public int CreditDay { get; set; }
        public bool HasRadara { get; set; }
        public bool WithDataTransfer { get; set; }
    }
}
