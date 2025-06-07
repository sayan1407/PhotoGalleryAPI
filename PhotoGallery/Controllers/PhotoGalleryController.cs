using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Data;
using PhotoGallery.Model;
using PhotoGallery.Model.DTO;

namespace PhotoGallery.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhotoGalleryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly string _rootPath;
        public PhotoGalleryController(ApplicationDbContext db)
        {
            _db = db;
            _rootPath = Path.GetFullPath("C:\\Users\\Sayan\\OneDrive\\Desktop\\React Project\\PhotoGalary\\photogallery\\public\\images");
        }

        [HttpGet("{userId}")]
        public ActionResult<IEnumerable<PhotoDTO>> GetPhotoes(string userId)
        {
            PhotoDTO photo = new();
            photo.Photos = _db.Photos.ToList();
            photo.LikedPhotos = _db.PhotoUserAssoc.Where(x => x.UserId == userId && x.likedislikeType).Select(p => p.Photo).ToList();
            photo.DisLikedPhotos = _db.PhotoUserAssoc.Where(x => x.UserId == userId && !x.likedislikeType).Select(p => p.Photo).ToList();


            return Ok(photo);
        }
        [HttpPost]
        public ActionResult CreatePhoto([FromForm]PhotoCreateDTO photoCreateDTO)
        {
            if(photoCreateDTO == null)
            {
                return BadRequest("Please upload a photo");
            }
            if(photoCreateDTO.file == null)
            {
                return BadRequest();
            }
            string filePath = Path.Combine(_rootPath, photoCreateDTO.file.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photoCreateDTO.file.CopyTo(fileStream);
            }
            Photo photo = new()
            {
                fileName = photoCreateDTO.file.FileName,
                Like = 0,
                Dislike = 0
            };
            _db.Photos.Add(photo);
            _db.SaveChanges();
            return Ok();

        }
        [HttpPut("{id:int}/{userId}/{type}")]
        public ActionResult UpdateLikeDislike(int id,string userId,string type)
        {
            if(id == 0)
            {
                return BadRequest();
            }
            Photo photo = _db.Photos.FirstOrDefault(p => p.Id == id);
            if (photo == null)
                return NotFound();
            PhotoUserAssoc photoUserAssoc = _db.PhotoUserAssoc.FirstOrDefault(p => p.UserId == userId && p.PhotoId == id);

            if (type.ToUpper() == "LIKE")
            {
                if (photoUserAssoc == null)
                {
                    PhotoUserAssoc newPhotoUserAssoc = new()
                    {
                        UserId = userId,
                        PhotoId = id,
                        likedislikeType = true
                    };
                    _db.PhotoUserAssoc.Add(newPhotoUserAssoc);
                    photo.Like++;
                }
                else {
                    if (photoUserAssoc.likedislikeType)
                    {
                        _db.PhotoUserAssoc.Remove(photoUserAssoc);
                        photo.Like--;
                    }
                    else
                    {
                        photoUserAssoc.likedislikeType = true;
                        photo.Like++;
                        photo.Dislike--;
                    }
                }
                
            }
            if (type.ToUpper() == "DISLIKE")
            {
                if (photoUserAssoc == null)
                {
                    PhotoUserAssoc newPhotoUserAssoc = new()
                    {
                        UserId = userId,
                        PhotoId = id,
                        likedislikeType = false
                    };
                    _db.PhotoUserAssoc.Add(newPhotoUserAssoc);
                    photo.Dislike++;
                }
                else
                {
                    if (!photoUserAssoc.likedislikeType)
                    {
                        _db.PhotoUserAssoc.Remove(photoUserAssoc);
                        photo.Dislike--;
                        
                    }
                    else
                    {
                        photoUserAssoc.likedislikeType = false;
                        photo.Like--;
                        photo.Dislike++;
                    }
                }
            }
            _db.SaveChanges();
            return Ok();
        }
    }
}
