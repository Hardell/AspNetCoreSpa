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
        protected readonly ApplicationDbContext DbContext;

        protected readonly UserManager<ApplicationUser> UserManager;

        protected readonly OnlineUserManager OnlineUserManager;

        protected Command(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, OnlineUserManager onlineUserManager)
        {
            DbContext = dbContext;
            UserManager = userManager;
            OnlineUserManager = onlineUserManager;
        }

        public abstract Task Execute(Hub hub);
    }
}
