using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController(
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IMapper mapper
    ) : BaseApiController
    {
        [HttpPost()]
        public async Task<ActionResult> AddMessage(CreateMessageDto messageDto)        
        {
            var username = User.GetUsername();

            if(username == messageDto.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot message yourself!!!");
            }

            var sender = await userRepository.GetUserByUserameAsync(username);
            var recipient = await userRepository.GetUserByUserameAsync(messageDto.RecipientUsername);

            if(sender == null || recipient == null || sender.UserName == null || recipient.UserName == null) return BadRequest("Cannot send message at this time!!!");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = messageDto.Content
            };

            messageRepository.AddMessage(message);

            if(await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to save message!!!");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages([FromQuery]MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var messages = await messageRepository.GetMessageThread(User.GetUsername(), username);

            return Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await messageRepository.GetMessage(id);

            if(message == null) return BadRequest("Cannot delete this message");

            if(message.SenderUsername != username && message.RecipientUsername != username)
                return Forbid();

            if(message.SenderUsername == username) message.SenderDeleted = true;
            if(message.RecipientUsername == username) message.RecipientDeleted = true;

            if(message is {SenderDeleted: true, RecipientDeleted: true}){
                messageRepository.DeleteMessage(message);
            }

            if(await messageRepository.SaveAllAsync()) return Ok();


            return BadRequest("Problem deleting the message");
        }
    }
}