namespace PhotoGallery.Model.DTO
{
    public class PhotoDTO
    {
        public List<Photo> Photos { get; set; }
        public List<Photo> LikedPhotos { get; set; }
        public List<Photo> DisLikedPhotos { get; set; }
        public List<Photo> UploadedPhotos { get; set; }

    }
}
