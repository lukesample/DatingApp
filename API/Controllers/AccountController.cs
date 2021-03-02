using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    //inherit from our BaseApiController; it already has the attributes we need
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        //use the Ctrl + . method to initialize properties from parameters (parameters in the ctor, i.e. context, then ctrl . and initialize)
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        //when sending in paramters, it needs to be an object; not strings!
        //we can use DTOs for this
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //when we use ActionResult we can return HTTP response codes 
            //BadRequest is a 400 error
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken.");

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user); //here we tell EF that we want to add it; this is telling EF to track this
            await _context.SaveChangesAsync(); //here, we actually save it to the database

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt); //use the overloaded function that takes a byte array key (password salt)

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password.");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}