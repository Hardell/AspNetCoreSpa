using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AspNetCoreSpa.Web.SignalR
{
    [Authorize]
    public class Chat : Hub
    {
        private static readonly Dictionary<string, OnlineUserStatus> onlineUsers = new Dictionary<string, OnlineUserStatus>();
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public Chat(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task Send(string message)
        {
            var username = Context.User.Identity.Name;

            if (message.StartsWith(".examine"))
            {
                var room = dbContext.Rooms.First(r => r.Name == onlineUsers[username].currentRoom);

                await Clients.Caller.SendAsync("send", JsonConvert.SerializeObject(room));
                return;
            }

            await Clients.Group(onlineUsers[username].currentRoom).SendAsync("send", message);
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            var user = this.dbContext.ApplicationUsers.Include(u => u.Room).First(u => u.UserName == username);

            await Groups.AddToGroupAsync(Context.ConnectionId, user.Room.Name);
            if (!onlineUsers.ContainsKey(username))
            {
                onlineUsers.Add(username, new OnlineUserStatus { ConnectionIds = new HashSet<string> { Context.ConnectionId }, LastAction = DateTimeOffset.UtcNow, currentRoom = user.Room.Name });
            }
            else
            {
                onlineUsers[username].ConnectionIds.Add(Context.ConnectionId);
            }

            await Clients.All.SendAsync("send", $"{user.UserName} sa prihlasil do ChatuQ.");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, onlineUsers[username].currentRoom);
            onlineUsers[username].ConnectionIds.Remove(Context.ConnectionId);
            if(onlineUsers[username].ConnectionIds.Count == 0)
            {
                onlineUsers.Remove(username);
            }


            await base.OnDisconnectedAsync(exception);
        }
    }
}