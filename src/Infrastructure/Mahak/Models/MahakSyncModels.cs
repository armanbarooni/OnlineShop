using System.Collections.Generic;

namespace OnlineShop.Infrastructure.Mahak.Models
{
    public class RequestAllDataModel
    {
        public long FromProductVersion { get; set; }
        public long FromPictureVersion { get; set; }
        public long FromPhotoGalleryVersion { get; set; }
        // Add other Versions as needed
    }

    public class MahakApiResult<T>
    {
        public bool Result { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }
    }

    public class GetAllDataResponse
    {
        public CommitDataModel Objects { get; set; }
    }

    public class CommitDataModel
    {
        public List<ProductModel>? Products { get; set; }
        public List<PictureModel>? Pictures { get; set; }
        public List<PhotoGalleryModel>? PhotoGalleries { get; set; }
        // Add other lists as needed
using System.Collections.Generic;

namespace OnlineShop.Infrastructure.Mahak.Models
{
    public class RequestAllDataModel
    {
        public long FromProductVersion { get; set; }
        public long FromPictureVersion { get; set; }
        public long FromPhotoGalleryVersion { get; set; }
        // Add other Versions as needed
    }

    public class MahakApiResult<T>
    {
        public bool Result { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }
    }

    public class GetAllDataResponse
    {
        public CommitDataModel Objects { get; set; }
    }

    public class CommitDataModel
    {
        public List<ProductModel>? Products { get; set; }
        public List<PictureModel>? Pictures { get; set; }
        public List<PhotoGalleryModel>? PhotoGalleries { get; set; }
        // Add other lists as needed
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
