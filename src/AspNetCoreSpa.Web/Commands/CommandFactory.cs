using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure;
using AspNetCoreSpa.Infrastructure.OnlineUserManager;
using AspNetCoreSpa.Web.Commands.Implementations;
using Microsoft.AspNetCore.Identity;
using System;
using System.Globalization;
using System.Linq;

namespace AspNetCoreSpa.Web.Commands
{
    public class CommandFactory
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly OnlineUserManager onlineUserManager;

        public CommandFactory(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, OnlineUserManager onlineUserManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.onlineUserManager = onlineUserManager;
        }

        public Command Parse(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidOperationException("Zly prikaz.");
            }

            if (!message.StartsWith(".") || message.StartsWith("..."))
            {
                return new TalkCommand(this.dbContext, this.userManager, this.onlineUserManager, message);
            }

            var command = message.Split(" ").First();
            
            if (string.Compare(command, ".go", CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
            {
                return new GoCommand(this.dbContext, this.userManager, this.onlineUserManager, message.Length <= 4 ? null : message.Substring(4));
            }
            if (string.Compare(command, ".talk", CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
            {
                return new TalkCommand(this.dbContext, this.userManager, this.onlineUserManager, message.Length <= 6 ? null : message.Substring(6));
            }
            if (string.Compare(command, ".look", CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
            {
                return new LookCommand(this.dbContext, this.userManager, this.onlineUserManager);
            }

            throw new InvalidOperationException("Zly prikaz."); // return an error command
        }
    }
}
