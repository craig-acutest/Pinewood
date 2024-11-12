using Microsoft.AspNetCore.Mvc;
using Pinewood.Models;
using Pinewood.Services;
using System.Threading.Tasks;

namespace Pinewood.Controllers
{
    public class LoginController : Controller
    {
        private readonly ITokenService _tokenService;

        public LoginController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid login request.");
            }

            try
            {
                var token = await _tokenService.LoginAsync(loginRequest);
                                
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                };

                Response.Cookies.Append("PinewoodAuthToken", token, cookieOptions);

                return Ok(new { Message = "Authentication successful." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }
    }
}
