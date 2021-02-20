using System;
using System.Collections.Generic;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class OnlineUserStatus
    {
        public OnlineUserStatus(string username,
            DateTimeOffset lastAction,
            HashSet<string> connectionIds,
            string currentRoomName,
            StopWatchWithOffset timeAccumulated,
            int money,
            ushort moneyAssignedCounter)
        {
            Username = username;
            LastAction = lastAction;
            ConnectionIds = connectionIds;
            CurrentRoomName = currentRoomName;
            TimeAccumulated = timeAccumulated;
            Money = money;
            MoneyAssignedCounter = moneyAssignedCounter;
        }

        public string Username { get; }

        public DateTimeOffset LastAction { get; set; }

        public HashSet<string> ConnectionIds { get; }

        public string CurrentRoomName { get; set; }

        public StopWatchWithOffset TimeAccumulated { get; }
        
        public int Money { get; set; }

        public ushort MoneyAssignedCounter { get; set; }
    }
}
