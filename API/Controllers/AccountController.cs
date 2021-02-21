using System.Security.Claims;
using System.Threading.Tasks;
using API.DTO;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 TokenService tokenService)
        {
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await
                        _userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(
                        user,
                        loginDTO.Password,
                        false
                    );

            if (result.Succeeded) return CreateUserDto(user);

            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await _userManager.Users.AnyAsync(_ => _.Email == registerDTO.Email))
            {
                return BadRequest("This email has been taken");
            }

            if (await _userManager.Users.AnyAsync(_ => _.UserName == registerDTO.Username))
            {
                return BadRequest("This username has been taken");
            }

            var user = new AppUser
            {
                DisplayName = registerDTO.DisplayName,
                Email = registerDTO.Email,
                UserName = registerDTO.Username,
            };

            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                return CreateUserDto(user);
            }

            return BadRequest("There was a problem registering user");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<UserDTO>> getCurrentUser()
        {
            string currentUserEmail = 
                    User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager
                            .FindByEmailAsync(currentUserEmail);

            return CreateUserDto(user);
        }

        private ActionResult<UserDTO> CreateUserDto(AppUser user)
        {
            return new UserDTO
            {
                DisplayName = user.DisplayName,
                Image = null,
                Token = _tokenService.CreateToken(user),
                Username = user.UserName
            };
        }
    }
}