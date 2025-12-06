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
        public string PackageNo { get; set; } = string.Empty; // From chat logs
        public long DatabaseId { get; set; } // From chat logs
    }
    
    public class LoginResultModel
    {
         public string Token { get; set; }
         // Add other fields if needed
    }
}
