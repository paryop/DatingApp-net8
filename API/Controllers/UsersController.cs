using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(
        IUserRepository userRepository, 
        IMapper mapper, 
        IPhotoService photoService): BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            // var users = await userRepository.GetUsersAsync();

            userParams.CurrentUsername = User.GetUsername();

            var userDtos = await userRepository.GetMembersAsync(userParams); //mapper.Map<IEnumerable<MemberDto>>(users);
            
            Response.AddPaginationHeader(userDtos);
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

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUserameAsync(User.GetUsername());

            if(user == null) return BadRequest("Cannot update user!!!");

            var result = await photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await userRepository.SaveAllAsync()) 
            return CreatedAtAction(nameof(GetUser), 
            new {username = user.UserName}, mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem uploading photo!!!");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserameAsync(User.GetUsername());

            if(user == null) return BadRequest("Could not find user!!!");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo!!!");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if(currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if(await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting main photo!!!");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
             var user = await userRepository.GetUserByUserameAsync(User.GetUsername());

            if(user == null) return BadRequest("Could not find user!!!");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null || photo.IsMain) return BadRequest("Cannot delete this as main photo!!!");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);         
            }
            
            user.Photos.Remove(photo);

            if(await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem deleting photo!!!");
        }
    }
}