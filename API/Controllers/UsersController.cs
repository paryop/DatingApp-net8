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
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IPhotoService photoService): BaseApiController
    {
        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            // var users = await unitOfWork.UserRepository.GetUsersAsync();

            userParams.CurrentUsername = User.GetUsername();

            var userDtos = await unitOfWork.UserRepository.GetMembersAsync(userParams); //mapper.Map<IEnumerable<MemberDto>>(users);
            
            Response.AddPaginationHeader(userDtos);
            return Ok(userDtos);

        }

        [Authorize(Roles = "Member")]
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto?>> GetUser(string username)
        {
            var currentUsername = User.GetUsername();

            var userDto = await unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: currentUsername == username); //mapper.Map<MemberDto>(user);

            return userDto;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberDto)
        {
            var username = User.GetUsername(); //User.FindFirst(ClaimTypes.Name)?.Value;

            if(username == null) return BadRequest("No username found in token");

            var user = await unitOfWork.UserRepository.GetUserByUserameAsync(username);

            if(user == null) return BadRequest("Could not find user");

            mapper.Map(memberDto,user);

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update member details!!!");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.UserRepository.GetUserByUserameAsync(User.GetUsername());

            if(user == null) return BadRequest("Cannot update user!!!");

            var result = await photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            // if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await unitOfWork.Complete()) 
            return CreatedAtAction(nameof(GetUser), 
            new {username = user.UserName}, mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem adding photo!!!");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUserByUserameAsync(User.GetUsername());

            if(user == null) return BadRequest("Could not find user!!!");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo!!!");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if(currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if(await unitOfWork.Complete()) return NoContent();

            return BadRequest("Problem setting main photo!!!");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
             var user = await unitOfWork.UserRepository.GetUserByUserameAsync(User.GetUsername());

            if(user == null) return BadRequest("Could not find user!!!");

            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if(photo == null || photo.IsMain) return BadRequest("Cannot delete this as main photo!!!");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);         
            }
            
            user.Photos.Remove(photo);

            if(await unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting photo!!!");
        }
    }
}