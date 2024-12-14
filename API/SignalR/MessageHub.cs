using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub(
        IMessageRepository messageRepository, 
        IUserRepository userRepository,
        IMapper mapper,
        IHubContext<PresenceHub> hubContext) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["user"];

            if(Context.User == null || string.IsNullOrEmpty(otherUser)) 
                throw new Exception("Cannot join group");

            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await messageRepository.GetMessageThread(Context.User.GetUsername(),otherUser!);
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public async Task SendMessage(CreateMessageDto messageDto)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("Could not get user!!!");

            if(username == messageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot message yourself!!!");
            }

            var sender = await userRepository.GetUserByUserameAsync(username);
            var recipient = await userRepository.GetUserByUserameAsync(messageDto.RecipientUsername);

            if(sender == null || recipient == null || sender.UserName == null || recipient.UserName == null) 
                throw new HubException("Cannot send message at this time!!!");
            
            
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = messageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await messageRepository.GetMessageGroup(groupName);

            if(group != null && group.Connections.Any(x => x.UserName == recipient.UserName))
            {
                message.DataRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionForUser(recipient.UserName);
                if(connections != null && connections?.Count != null)
                {
                    await hubContext.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new {username = sender.UserName, knownAs = sender.KnownAs});
                }
            }

            messageRepository.AddMessage(message);

            if(await messageRepository.SaveAllAsync()) 
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var username = Context.User?.GetUsername();
            var group = await messageRepository.GetMessageGroup(groupName);
            var connection = new Connection{ConnectionId = Context.ConnectionId, UserName = username!};

            if(group == null)
            {
                group = new Group{Name = groupName};
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);
            if(await messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to join group!!!");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if(connection != null && group != null)
            {
                messageRepository.RemoveConnection(connection);
                if(await messageRepository.SaveAllAsync()) return group;
            }

            throw new Exception("Failed to remove from group!!!");
        }

        private string GetGroupName(string caller, string? other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}