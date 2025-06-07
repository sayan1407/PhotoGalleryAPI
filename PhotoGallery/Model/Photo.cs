using System.ComponentModel.DataAnnotations;

namespace PhotoGallery.Model
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        public string fileName { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
    }
}
