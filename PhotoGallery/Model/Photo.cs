using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoGallery.Model
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        public string fileName { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
        
        public string? UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public ApplicationUser? User { get; set; }
    }
}
