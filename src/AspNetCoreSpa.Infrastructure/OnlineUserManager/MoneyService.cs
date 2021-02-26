using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class MoneyService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OnlineUserManager _onlineUserManager;
        private readonly ILogger<MoneyService> _logger;
        private int _moneyPerHour = 333;
        private int _delayInMs = 30000;

        public MoneyService(ApplicationDbContext dbContext, OnlineUserManager onlineUserManager, ILogger<MoneyService> logger)
        {
            _dbContext = dbContext;
            _onlineUserManager = onlineUserManager;
            _logger = logger;
        }

        public async Task Count(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _moneyPerHour = _dbContext.ChatQSettings.First().MoneyPerHour;
                    _delayInMs = _dbContext.ChatQSettings.First().MoneyServiceDelay;
                }
                catch (Exception e)
                {
                    // log sth
                }
                
                _onlineUserManager.AddMoneyForTimeAccumulated(_moneyPerHour);
                
                await Task.Delay(_delayInMs, cancellationToken);   
            }
        }
    }
}
