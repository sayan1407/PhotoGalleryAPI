using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PhotoGallery.Data;
using PhotoGallery.Model.DTO;
using PhotoGallery.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace PhotoGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private string apiKey;
        public AuthController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _db = db;
            _userManager = userManager;
            _response = new ApiResponse();
            apiKey = configuration.GetValue<string>("ApiSettings:SecretKey");

        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequestDTO model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model == null)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        return BadRequest(_response);
                    }
                    var userInDb = _userManager.Users.SingleOrDefault(x => x.UserName == model.UserName);
                    if (userInDb != null)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessages.Add("User name already exist");
                        return BadRequest(_response);
                    }
                    ApplicationUser applicationUser = new()
                    {
                        UserName = model.UserName,
                        NormalizedUserName = model.UserName.ToUpper(),
                        Email = model.UserName,
                        Name = model.UserName,
                    };
                    var result = await _userManager.CreateAsync(applicationUser, model.Password);
                    if (result.Succeeded)
                    {
                        

                        _response.IsSuccess = true;
                        _response.StatusCode = HttpStatusCode.OK;
                        return Ok(_response);

                    }
                    else
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessages.AddRange(result.Errors.Select(x => x.Description).ToList());
                        return BadRequest(_response);
                    }
                }
                catch (Exception ex)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add(ex.Message);
                    return BadRequest(_response);
                }

            }
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("Error while registaring");
            return BadRequest(_response);
        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequestDTO model)
        {
            if (model == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var userInDb = _userManager.Users.SingleOrDefault(u => u.UserName.ToUpper() == model.UserName.ToUpper());
            bool isValid = await _userManager.CheckPasswordAsync(userInDb, model.Password);
            if (isValid)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                JwtSecurityTokenHandler tokenHandler = new();
                byte[] key = Encoding.ASCII.GetBytes(apiKey);
                SecurityTokenDescriptor securityTokenDescriptor = new()
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                    {
                        new Claim("id",userInDb.Id.ToString()),
                        new Claim(ClaimTypes.Email,userInDb.UserName),
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                SecurityToken token = tokenHandler.CreateToken(securityTokenDescriptor);
                var loginResponse = new LoginResponseDTO()
                {
                    Email = userInDb.Email,
                    Token = tokenHandler.WriteToken(token),
                };
                if (string.IsNullOrEmpty(loginResponse.Token))
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Fail to generate JWT token");
                    return BadRequest(_response);
                }
                else
                {
                    _response.IsSuccess = true;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = loginResponse;
                    return Ok(_response);
                }
            }
            else
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("User name or password is invalid");
                return BadRequest(_response);
            }
        }
    }

}
