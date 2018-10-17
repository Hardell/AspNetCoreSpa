using AspNetCoreSpa.Core.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class OnlineUserManager
    {
        private readonly ConcurrentDictionary<string, OnlineUserStatus> onlineUsers = new ConcurrentDictionary<string, OnlineUserStatus>();
        private ConcurrentDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>();

        public OnlineUserStatus GetUserStatus(string username)
        {
            return this.onlineUsers[username];
        }

        public string GetUserRoomName(string username)
        {
            return this.onlineUsers[username].CurrentRoomName;
        }
        
        public void AddUserStatus(string username, OnlineUserStatus userStatus)
        {
            if(!this.onlineUsers.TryAdd(username, userStatus))
            {
                throw new InvalidOperationException($"Couldn't add {userStatus.Username} to the dictionary");
            }
        }

        public void RenewUserLastAction(string username)
        {
            var key = dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                this.onlineUsers[username].LastAction = DateTimeOffset.UtcNow;
            }
        }

        public void UpdateUserRoom(string username, string roomName)
        {
            var key = dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                this.onlineUsers[username].CurrentRoomName = roomName;
            }
        }

        public void AddUserConnectionId(string username, string connectionId)
        {
            var oldSet = this.GetUserStatus(username).ConnectionIds;

            var key = dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                this.onlineUsers[username].ConnectionIds.Add(connectionId);
            }
        }

        public void RemoveUserConnectionId(string username, string connectionId)
        {
            var oldSet = this.GetUserStatus(username).ConnectionIds;

            var key = dictionary.GetOrAdd(username, new object());
            lock (key)
            {
                this.onlineUsers[username].ConnectionIds.Remove(connectionId);
            }
        }

        public bool IsUserOnline(string username)
        {
            return this.onlineUsers.ContainsKey(username);
        }

    }
}
