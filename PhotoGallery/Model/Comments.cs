using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoGallery.Model
{
    public class Comments
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        
        public int PhotoId { get; set; }
        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }
        public string Comment { get; set; }
    }
}
