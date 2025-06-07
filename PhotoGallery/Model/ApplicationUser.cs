using Microsoft.AspNetCore.Identity;

namespace PhotoGallery.Model
{
    public class ApplicationUser :IdentityUser
    {
        public string Name { get; set; }
    }
}
