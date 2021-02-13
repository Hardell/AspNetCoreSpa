using Newtonsoft.Json;

namespace AspNetCoreSpa.Core.Entities
{
    public class RoomEdge
    {
        [JsonIgnore]
        public Room Room { get; set; }
        public int RoomId { get; set; }

        [JsonIgnore]
        public Room AdjacentRoom { get; set; }
        public int AdjacentRoomId { get; set; }
    }
}
