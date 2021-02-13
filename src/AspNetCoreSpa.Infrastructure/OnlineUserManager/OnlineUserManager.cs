using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class OnlineUserManager
    {
        private readonly ConcurrentDictionary<string, OnlineUserStatus> _onlineUsers = new ConcurrentDictionary<string, OnlineUserStatus>();
        private readonly ConcurrentDictionary<string, object> _dictionary = new ConcurrentDictionary<string, object>();

        public OnlineUserStatus GetUserStatus(string username)
        {
            return _onlineUsers[username];
        }

        public string GetUserRoomName(string username)
        {
            return _onlineUsers[username].CurrentRoomName;
        }

        public List<string> GetUsernamesInRoom(string roomName)
        {
            return _onlineUsers.Values.Where(s => s.CurrentRoomName == roomName).Select(s => s.Username).ToList();
        }

        public void AddUserStatus(string username, OnlineUserStatus userStatus)
        {
            if (!_onlineUsers.TryAdd(username, userStatus))
            {
                throw new InvalidOperationException($"Couldn't add {userStatus.Username} to the dictionary");
            }
        }

        public OnlineUserStatus RemoveUserStatus(string username)
        {
            if (!_onlineUsers.TryRemove(username, out var result))
            {
                throw new InvalidOperationException($"Couldn't remove {username} from the dictionary");
            }

            return result;
        }

        public void RenewUserLastAction(string username)
        {
            var key = _dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                _onlineUsers[username].LastAction = DateTimeOffset.UtcNow;
            }
        }

        public void UpdateUserRoom(string username, string roomName)
        {
            var key = _dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                _onlineUsers[username].CurrentRoomName = roomName;
            }
        }

        public void AddUserConnectionId(string username, string connectionId)
        {
            var key = _dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                _onlineUsers[username].ConnectionIds.Add(connectionId);
            }
        }

        public OnlineUserStatus RemoveUserConnectionId(string username, string connectionId)
        {
            var key = _dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                _onlineUsers[username].ConnectionIds.Remove(connectionId);
            }
            
            return !_onlineUsers[username].ConnectionIds.Any() ? RemoveUserStatus(username) : _onlineUsers[username];
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

    }
}
