using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreSpa.Core.Entities
{
    public class OnlineUserStatus
    {
        public DateTimeOffset LastAction { get; set; }

        public HashSet<string> ConnectionIds { get; set; }

        public string currentRoom { get; set; }
    }
}
