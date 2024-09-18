using System.Security.Claims;
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

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(username == null) return BadRequest("No username found in token");

            var user = await userRepository.GetUserByUserameAsync(username);

            if(user == null) return BadRequest("Could not find user");

            mapper.Map(memberDto,user);

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update member details!!!");
        }
    }
}