using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure;
using AspNetCoreSpa.Infrastructure.OnlineUserManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace AspNetCoreSpa.Web.Commands.Implementations
{
    public class TalkCommand : Command
    {
        private readonly string message;

        public TalkCommand(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, OnlineUserManager onlineUserManager, string message) 
            : base(dbContext, userManager, onlineUserManager)
        {
            this.message = message;
        }

        public override Task Execute(Hub hub)
        {
            var username = hub.Context.User.Identity.Name;

            if (string.IsNullOrWhiteSpace(message))
            {
                var error = new CommandResponse { Command = "error", Content = "Musis nieco povedat." };
                return hub.Clients.Caller.SendAsync("send", JsonConvert.SerializeObject(error));
            }

            var response = new CommandResponse { Command = "talk", Content = username + ": " + message };
            return hub.Clients.Group(this.onlineUserManager.GetUserRoomName(username)).SendAsync("send", JsonConvert.SerializeObject(response));
        }
    }
}
