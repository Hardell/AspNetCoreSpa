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
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly CommandFactory commandFactory;
        private readonly OnlineUserManager onlineUserManager;

        public Chat(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, CommandFactory commandFactory, OnlineUserManager onlineUserManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.commandFactory = commandFactory;
            this.onlineUserManager = onlineUserManager;
        }

        public Task Send(string message)
        {
            var command = this.commandFactory.Parse(message);
            return command.Execute(this);
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            var currentRoomName = this.dbContext.ApplicationUsers.Include(u => u.Room).Single(u => u.UserName == username).Room.Name;


            await Groups.AddToGroupAsync(Context.ConnectionId, currentRoomName);
            if (!this.onlineUserManager.IsUserOnline(username))
            {
                var status = new OnlineUserStatus(username, DateTimeOffset.UtcNow, new HashSet<string> { Context.ConnectionId }, currentRoomName);
                this.onlineUserManager.AddUserStatus(username, status);
            }
            else
            {
                this.onlineUserManager.AddUserConnectionId(username, Context.ConnectionId);
            }

            await Clients.All.SendAsync("send", $"{username} sa prihlasil do ChatuQ.");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, this.onlineUserManager.GetUserRoomName(username));
            this.onlineUserManager.RemoveUserConnectionId(username, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}