using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoGallery.Model
{
    public class PhotoUserAssoc
    {
        public int Id { get; set; }
     
        public int PhotoId { get; set; }
        [ForeignKey(nameof(PhotoId))]
        public Photo  Photo{ get; set; }
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }
        public bool likedislikeType { get; set; }
    }
}
