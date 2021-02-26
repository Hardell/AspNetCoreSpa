using System;
using System.Threading.Tasks;
using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure.OnlineUserManager;
using AspNetCoreSpa.Web.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCoreSpa.Web.SignalR
{
    [Authorize]
    public class Chat : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CommandFactory _commandFactory;
        private readonly OnlineUserManager _onlineUserManager;

        public Chat( UserManager<ApplicationUser> userManager, CommandFactory commandFactory, OnlineUserManager onlineUserManager)
        {
            _userManager = userManager;
            _commandFactory = commandFactory;
            _onlineUserManager = onlineUserManager;
        }

        public Task Send(string message)
        {
            var command = _commandFactory.Parse(message);
            return command.Execute(this);
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            var roomName = _onlineUserManager.GetUserRoomName(username);
            if (roomName == null)
            {
                var user = await _userManager.FindByNameAsync(username);
                roomName = user.RoomId; //todo Add a default room if the existing room doesn't exist anymore.
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            await _onlineUserManager.AddUserConnectionId(username, Context.ConnectionId, _userManager);
            await Clients.All.SendAsync("send", $"{username} sa prihlasil do ChatuQ.");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _onlineUserManager.GetUserRoomName(username));
            await _onlineUserManager.RemoveUserConnectionId(username, Context.ConnectionId, _userManager);

            if (!_onlineUserManager.IsUserOnline(username))
            {
                await Clients.All.SendAsync("send", $"{username} sa odhlasil z ChatQ.");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}