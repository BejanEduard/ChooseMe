using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext _context, ITokenService _tokenService)
        {
            context = _context;
            tokenService = _tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoggedUser>> Register(RegisterDto _registerDto)
        {
            if (await this.UserExists(_registerDto.UserName)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            var user = new Entities.User
            {
                UserName = _registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(_registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            context.Users.Add(user);

            await context.SaveChangesAsync();

            return new LoggedUser
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }
    

        [HttpPost("login")]
        public async Task<ActionResult<LoggedUser>> Login(LoginDto _loginDto)
        {
            var user = await context.Users.SingleOrDefaultAsync(u => u.UserName == _loginDto.UserName);

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(_loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new LoggedUser
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }



        private async Task<bool> UserExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
