using System;
using System.Collections.Generic;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class OnlineUserStatus
    {
        public OnlineUserStatus(string username, DateTimeOffset lastAction, HashSet<string> connectionIds, string currentRoomName, StopWatchWithOffset stopWatch)
        {
            Username = username;
            LastAction = lastAction;
            ConnectionIds = connectionIds;
            CurrentRoomName = currentRoomName;
            StopWatch = stopWatch;
        }

        public string Username { get; }

        public DateTimeOffset LastAction { get; set; }

        public HashSet<string> ConnectionIds { get; }

        public string CurrentRoomName { get; set; }

        public StopWatchWithOffset StopWatch { get; }
    }
}
