namespace OnlineShop.Infrastructure.Mahak.Models
{
    /// <summary>
    /// Model for Picture data from Mahak (incoming)
    /// </summary>
    public class PictureModel
    {
        public int PictureId { get; set; }
        public string? Url { get; set; }
        public string? FileName { get; set; }
        public string? FileExtension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Size { get; set; }
        public string? MimeType { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
        public int PictureClientId { get; set; }
        public int PictureCode { get; set; }
    }

    /// <summary>
    /// Model for sending Picture to Mahak (outgoing)
    /// </summary>
    public class MahakPictureModel
    {
        public long PictureClientId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string BinaryData { get; set; } = string.Empty; // Base64 without prefix
        public bool Deleted { get; set; } = false;
    }

    public class PhotoGalleryModel
    {
        public int PhotoGalleryId { get; set; }
        public int PhotoGalleryClientId { get; set; }
        public int PhotoGalleryCode { get; set; }
        public int EntityType { get; set; } // 101: Person, 102: Product
        public int ItemCode { get; set; }
        public int FromPictureVersion { get; set; } // Note: Check if this is a field in the model or just request
        public int PictureId { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
        public bool IsMain { get; set; }
    }
}
