using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(
        DataContext context, 
        ITokenService tokenService, 
        IMapper mapper) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {

            if(await UserExists(registerDto.Username))
            {
                return BadRequest("User already exists!!!");
            }
            
            using var hmac = new HMACSHA512();

            var user = mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.Username;
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            // var user = new AppUser{
            //     UserName = registerDto.Username,
            //     PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            //     PasswordSalt = hmac.Key
            // };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new UserDto{
                Username = user.UserName,
                Gender = user.Gender,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }  

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await context.Users
            .Include("Photos")
            .FirstOrDefaultAsync(u => u.UserName == loginDto.Username.ToLower());

            if(user == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash =hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0; i<computedHash.Length; i++)
            {
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password !!!");
            }
            
            return new UserDto{
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=> x.IsMain)?.Url
            };
        }

        private async Task<bool> UserExists(string username)    
        {
            return await context.Users.AnyAsync(u => u.UserName == username);
        }  
    }
}