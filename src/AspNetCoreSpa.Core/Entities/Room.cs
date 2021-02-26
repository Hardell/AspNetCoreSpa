using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreSpa.Core.Entities
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(250)]
        public string Id { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        public ChatRoomPhoto Photo { get; set; }

        public ICollection<RoomEdge> AdjacentRooms { get; set; } = new List<RoomEdge>();
    }
}
