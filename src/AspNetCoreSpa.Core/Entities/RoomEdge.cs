namespace AspNetCoreSpa.Core.Entities
{
    public class RoomEdge
    {
        public Room Room { get; set; }
        public int RoomId { get; set; }
        public Room AdjacentRoom { get; set; }
        public int AdjacentRoomId { get; set; }
    }
}
