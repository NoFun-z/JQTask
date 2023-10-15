using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        private readonly JContext _context;
        public AccountController(UserManager<User> userManager, TokenService tokenService, JContext context)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
                return Unauthorized();

            var userListBoards = await RetrieveListBoards(loginDTO.Username);

            var listBoardsDTOs = userListBoards?.Select(u => u.MapListBoardToDTO()).ToList();

            return new UserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                ListBoards = listBoardsDTOs
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            var user = new User { UserName = registerDTO.Username, Email = registerDTO.Email };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user, "Member");

            return StatusCode(201);
        }

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userListBoards = await RetrieveListBoards(User.Identity.Name);

            var listBoardDTOs = userListBoards?.Select(listBoard => listBoard.MapListBoardToDTO()).ToList();

            return new UserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                ListBoards = listBoardDTOs
            };
        }

        private async Task<List<ListBoard>> RetrieveListBoards(string buyerId)
        {
            if (string.IsNullOrEmpty(buyerId))
            {
                Response.Cookies.Delete("buyerId");
                return null;
            }

            return await _context.ListBoards
                .Include(x => x.Tasklists)
                .ThenInclude(x => x.Tasks)
                .Where(c => c.BuyerIDs.Any(bid => bid.UserID == buyerId))
                .ToListAsync();
        }
    }
}