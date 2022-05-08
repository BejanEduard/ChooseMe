using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(DataContext _context, ITokenService _tokenService, IMapper _mapper)
        {
            context = _context;
            tokenService = _tokenService;
            mapper = _mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoggedUser>> Register(RegisterDto _registerDto)
        {
            if (await this.UserExists(_registerDto.UserName)) return BadRequest("Username is taken");

            var user = mapper.Map<User>(_registerDto);

            using var hmac = new HMACSHA512();

            user.UserName = _registerDto.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(_registerDto.Password));
            user.PasswordSalt = hmac.Key;

            context.Users.Add(user);

            await context.SaveChangesAsync();

            return new LoggedUser
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnowsAs
            };
        }
    

        [HttpPost("login")]
        public async Task<ActionResult<LoggedUser>> Login(LoginDto _loginDto)
        {
            var user = await context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(u => u.UserName == _loginDto.UserName);

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
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnowsAs
            };
        }



        private async Task<bool> UserExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
