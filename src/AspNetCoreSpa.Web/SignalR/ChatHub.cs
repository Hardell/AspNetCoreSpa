using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure;
using AspNetCoreSpa.Infrastructure.OnlineUserManager;
using AspNetCoreSpa.Web.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreSpa.Web.SignalR
{
    [Authorize]
    public class Chat : Hub
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CommandFactory _commandFactory;
        private readonly OnlineUserManager _onlineUserManager;

        public Chat(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, CommandFactory commandFactory, OnlineUserManager onlineUserManager)
        {
            _dbContext = dbContext;
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
            var user = _dbContext.ApplicationUsers.Include(u => u.Room).Single(u => u.UserName == username);
            var currentRoomName = user.Room.Name; //todo Add a default room if the existing room doesn't exist anymore.

            await Groups.AddToGroupAsync(Context.ConnectionId, currentRoomName);
            if (!_onlineUserManager.IsUserOnline(username))
            {
                var status = new OnlineUserStatus(username,
                    DateTimeOffset.UtcNow,
                    new HashSet<string> { Context.ConnectionId },
                    currentRoomName,
                    new StopWatchWithOffset(user.TimeAccumulated),
                    user.Money,
                    user.MoneyAssignedCounter);
                status.TimeAccumulated.Start();
                _onlineUserManager.AddUserStatus(username, status);
            }
            else
            {
                _onlineUserManager.AddUserConnectionId(username, Context.ConnectionId);
            }

            await Clients.All.SendAsync("send", $"{username} sa prihlasil do ChatuQ.");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _onlineUserManager.GetUserRoomName(username));
            var status = _onlineUserManager.RemoveUserConnectionId(username, Context.ConnectionId);

            if(!status.ConnectionIds.Any())
            {
                var user = await _userManager.FindByNameAsync(username);
                user.TimeAccumulated = status.TimeAccumulated.ElapsedTimeSpan;
                user.RoomId = _dbContext.Rooms.First(r => r.Name == status.CurrentRoomName).Id;
                user.MoneyAssignedCounter = status.MoneyAssignedCounter;
                user.Money = status.Money;
                
                await _userManager.UpdateAsync(user);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}