namespace AspNetCoreSpa.Core.Entities
{
    public class ChatRoomPhoto : PhotoBase
    {
        public int ChatRoomId { get; set; }

        public Room ChatRoom { get; set; }
    }
}
