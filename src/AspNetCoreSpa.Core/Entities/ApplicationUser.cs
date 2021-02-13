using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AspNetCoreSpa.Core.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public ApplicationUser()
        {
            TimeAccumulated = TimeSpan.Zero;
        }

        public bool IsEnabled { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string LastName { get; set; }

        [Phone]
        public string Mobile { get; set; }

        public ApplicationUserPhoto ProfilePhoto { get; set; }

        public Room Room { get; set; }

        public int RoomId { get; set; }

        public TimeSpan TimeAccumulated { get; set; }

        //public ICollection<Rank> Ranks { get; set; }

        [NotMapped]
        public string Name => FirstName + " " + LastName;
    }
}
