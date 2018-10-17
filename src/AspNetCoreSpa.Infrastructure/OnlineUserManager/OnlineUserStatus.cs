using AspNetCoreSpa.Core.Entities;
using System;
using System.Collections.Generic;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class OnlineUserStatus
    {
        public OnlineUserStatus(string username, DateTimeOffset lastAction, HashSet<string> connectionIds, string currentRoomName)
        {
            this.Username = username;
            this.LastAction = lastAction;
            this.ConnectionIds = connectionIds;
            this.CurrentRoomName = currentRoomName;
        }

        public string Username { get; }

        public DateTimeOffset LastAction { get; set; }

        public HashSet<string> ConnectionIds { get; }

        public string CurrentRoomName { get; set; }
    }
}
