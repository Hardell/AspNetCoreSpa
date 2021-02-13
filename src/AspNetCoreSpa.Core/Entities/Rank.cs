using System;
using System.Collections.Generic;

namespace AspNetCoreSpa.Core.Entities
{
    public class Rank
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime RequiredOnlineTime { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }
}
