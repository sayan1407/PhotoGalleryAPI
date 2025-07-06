using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private ApiResponse _response;
        public PhotoGalleryController(ApplicationDbContext db)
        {
            _db = db;
            _rootPath = Path.GetFullPath("C:\\Users\\Sayan\\OneDrive\\Desktop\\React Project\\PhotoGalary\\photogallery\\public\\images");
            _response = new ApiResponse();
        }

        [HttpGet("{userId}")]
        public ActionResult<IEnumerable<PhotoDTO>> GetPhotoes(string userId)
        {
            PhotoDTO photo = new();
            photo.Photos = _db.Photos.Include(p => p.User).ToList();
            photo.LikedPhotos = _db.PhotoUserAssoc.Where(x => x.UserId == userId && x.likedislikeType).Select(p => p.Photo).ToList();
            photo.DisLikedPhotos = _db.PhotoUserAssoc.Where(x => x.UserId == userId && !x.likedislikeType).Select(p => p.Photo).ToList();
            photo.UploadedPhotos = _db.Photos.Where(x => x.UserID == userId).ToList();


            return Ok(photo);
        }
        [HttpPost]
        public ActionResult CreatePhoto([FromForm]PhotoCreateDTO photoCreateDTO)
        {
            if(photoCreateDTO == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Photo is null");
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return Ok(_response);
            }
            if(photoCreateDTO.file == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Photo is null");
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return Ok(_response);
            }
            string fileName = photoCreateDTO.file.FileName.Substring(0, photoCreateDTO.file.FileName.IndexOf("."));
            string fileExtension = photoCreateDTO.file.FileName.Substring(photoCreateDTO.file.FileName.IndexOf("."));
            string newFileName = fileName+ "_" + Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(_rootPath, newFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photoCreateDTO.file.CopyTo(fileStream);
            }
            Photo photo = new()
            {
                fileName = newFileName,
                Like = 0,
                Dislike = 0,
                UserID = photoCreateDTO.UserId
            };
            _db.Photos.Add(photo);
            _db.SaveChanges();
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);

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
        [HttpGet("comments/{photoId}")]
        public ActionResult<ApiResponse> GetComments(int photoId)
        {
            if(photoId == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Photo Id is 0");
                return Ok(_response);
            }
            List<Comments> comments = _db.Comments.Where(c => c.PhotoId == photoId).Include(c => c.User).ToList();
            if(comments == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess=false;
                _response.ErrorMessages.Add("There are no comments");
                return Ok(_response);
            }
            _response.IsSuccess = true;
            _response.StatusCode=System.Net.HttpStatusCode.OK;
            _response.Result = comments;
            return Ok(_response);
        }
        [HttpPost("comments")]
        public ActionResult<ApiResponse> AddComment([FromBody]CreateCommentDTO commentDTO)
        {
            if(commentDTO.UserId == null || commentDTO.PhotoId == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User Id or Photo Id is 0");
                return Ok(_response);
            }
            if(string.IsNullOrEmpty(commentDTO.Comment))
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Comment can't be blank");
                return Ok(_response);

            }
            Comments comments = new Comments()
            {
                UserId = commentDTO.UserId,
                PhotoId = commentDTO.PhotoId,
                Comment = commentDTO.Comment,
            };
            _db.Comments.Add(comments);
            _db.SaveChanges();
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);

        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeletePhoto(int id)
        {
            try
            {
                Photo? photo = await _db.Photos.SingleOrDefaultAsync(p => p.Id == id);
                if (photo == null || photo.Id == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("photo can't be deleted");
                    return _response;
                }
                _db.Photos.Remove(photo);
                await _db.SaveChangesAsync();
                _response.IsSuccess = true;
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                return _response;

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add(ex.Message.ToString());
                return _response;

            }
            
        }
    }
}
