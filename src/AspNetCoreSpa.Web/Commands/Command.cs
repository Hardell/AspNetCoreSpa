using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure;
using AspNetCoreSpa.Infrastructure.OnlineUserManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AspNetCoreSpa.Web.Commands
{
    public abstract class Command
    {
        protected readonly ApplicationDbContext dbContext;

        protected readonly UserManager<ApplicationUser> userManager;

        protected readonly OnlineUserManager onlineUserManager;

        protected Command(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, OnlineUserManager onlineUserManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.onlineUserManager = onlineUserManager;
        }

        public abstract Task Execute(Hub hub);
    }
}
