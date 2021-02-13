using AspNetCoreSpa.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class OnlineTimeCounterService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly OnlineUserManager onlineUserManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<OnlineTimeCounterService> logger;

        public OnlineTimeCounterService(ApplicationDbContext dbContext, OnlineUserManager onlineUserManager, UserManager<ApplicationUser> userManager, ILogger<OnlineTimeCounterService> logger)
        {
            this.dbContext = dbContext;
            this.onlineUserManager = onlineUserManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task Count(CancellationToken cancellationToken)
        {
            var step = TimeSpan.FromSeconds(15);
            var sw = new Stopwatch();

            while (!cancellationToken.IsCancellationRequested)
            {
                sw.Restart();
                var onlineUsers = onlineUserManager.GetOnlineUserStatuses();
                var onlineUsernames = onlineUsers.Where(u => DateTimeOffset.UtcNow - u.LastAction < TimeSpan.FromMinutes(20)).Select(u => u.Username).ToList();
                var tasks = new List<Task<IdentityResult>>();

                var dbUsers = dbContext.Users.Where(u => onlineUsernames.Contains(u.UserName));

                foreach (var dbUser in dbUsers)
                {
                    dbUser.TimeAccumulated += step;
                    tasks.Add(userManager.UpdateAsync(dbUser));
                }

                await Task.WhenAll(tasks);
                logger.LogInformation($"Took {sw.Elapsed.TotalMilliseconds}!");
                await Task.Delay(step - sw.Elapsed, cancellationToken);   
            }
        }
    }
}
