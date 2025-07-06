using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PhotoGallery.Model.DTO
{
    public class CreateCommentDTO
    {
       
        public string UserId { get; set; }
        public int PhotoId { get; set; }
        
        public string Comment { get; set; }
    }
}
