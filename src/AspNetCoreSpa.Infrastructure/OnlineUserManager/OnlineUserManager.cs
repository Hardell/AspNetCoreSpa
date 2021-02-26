using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreSpa.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class OnlineUserManager
    {
        //private readonly UserManager<ApplicationUser> _userManager; // TODO figure this out
        private readonly ConcurrentDictionary<string, OnlineUserStatus> _onlineUsers = new ConcurrentDictionary<string, OnlineUserStatus>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        public OnlineUserStatus GetUserStatus(string username)
        {
            _onlineUsers.TryGetValue(username, out var status);

            return status;
        }

        public string GetUserRoomName(string username)
        {
            _onlineUsers.TryGetValue(username, out var status);

            return status?.CurrentRoomName;
        }

        public IEnumerable<string> GetUsernamesInRoom(string roomName)
        {
            return _onlineUsers.Values.Where(s => s.CurrentRoomName == roomName).Select(s => s.Username);
        }

        public async Task RenewUserLastAction(string username)
        {
            var semaphore = _lockDictionary.GetOrAdd(username, new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                _onlineUsers[username].LastAction = DateTimeOffset.UtcNow;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task UpdateUserRoom(string username, string roomName)
        {
            var semaphore = _lockDictionary.GetOrAdd(username, new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                _onlineUsers[username].CurrentRoomName = roomName;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task RemoveUserConnectionId(string username, string connectionId, UserManager<ApplicationUser> userManager)
        {
            var semaphore = _lockDictionary.GetOrAdd(username, new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                _onlineUsers[username].ConnectionIds.Remove(connectionId);

                if (!_onlineUsers[username].ConnectionIds.Any())
                {
                    await LogUserOff(username, userManager);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task AddUserConnectionId(string username, string connectionId, UserManager<ApplicationUser> userManager)
        {
            var semaphore = _lockDictionary.GetOrAdd(username, new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                if (IsUserOnline(username))
                {
                    _onlineUsers[username].ConnectionIds.Add(connectionId);
                }
                else
                {
                    var user = await userManager.FindByNameAsync(username);
                    var status = new OnlineUserStatus(username,
                        DateTimeOffset.UtcNow,
                        new HashSet<string> { connectionId },
                        user.RoomId,
                        new StopWatchWithOffset(user.TimeAccumulated),
                        user.Money,
                        user.MoneyAssignedCounter);
                    status.TimeAccumulated.Start();
                    AddUserStatus(username, status);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public bool IsUserOnline(string username)
        {
            return _onlineUsers.ContainsKey(username);
        }

        public IEnumerable<string> GetOnlineUsernames()
        {
            return _onlineUsers.Keys;
        }

        public IEnumerable<OnlineUserStatus> GetOnlineUserStatuses()
        {
            return _onlineUsers.Values;
        }

        public void AddMoneyForTimeAccumulated(int moneyPerHour)
        {
            var onlineUsers = GetOnlineUserStatuses();
            
            foreach (var onlineUser in onlineUsers)
            {
                var semaphore = _lockDictionary.GetOrAdd(onlineUser.Username, new SemaphoreSlim(1, 1));

                try
                {
                    if (onlineUser.MoneyAssignedCounter < onlineUser.TimeAccumulated.ElapsedTimeSpan.TotalHours)
                    {
                        onlineUser.Money += moneyPerHour;
                        onlineUser.MoneyAssignedCounter++;
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        private async Task LogUserOff(string username, UserManager<ApplicationUser> userManager)
        {
            if (_onlineUsers.TryRemove(username, out var status))
            {
                var user = await userManager.FindByNameAsync(username);
                user.TimeAccumulated = status.TimeAccumulated.ElapsedTimeSpan;
                user.RoomId = status.CurrentRoomName;
                user.MoneyAssignedCounter = status.MoneyAssignedCounter;
                user.Money = status.Money;
                
                await userManager.UpdateAsync(user);
            }
            else
            {
                throw new InvalidOperationException($"Couldn't remove {username} from the dictionary");
            }
        }
        
        private void AddUserStatus(string username, OnlineUserStatus userStatus)
        {
            if (!_onlineUsers.TryAdd(username, userStatus))
            {
                throw new InvalidOperationException($"Couldn't add {userStatus.Username} to the dictionary");
            }
        }
    }
}
