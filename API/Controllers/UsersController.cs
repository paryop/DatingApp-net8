using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper): BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            // var users = await userRepository.GetUsersAsync();

            var userDtos = await userRepository.GetMembersAsync(); //mapper.Map<IEnumerable<MemberDto>>(users);
            
            return Ok(userDtos);

        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto?>> GetUser(string username)
        {
            // var user = await userRepository.GetUserByUserameAsync(username);

            // if(user == null) return NotFound();

            var userDto = await userRepository.GetMemberAsync(username); //mapper.Map<MemberDto>(user);

            return userDto;
        }
    }
}