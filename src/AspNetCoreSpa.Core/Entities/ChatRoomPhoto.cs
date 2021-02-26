namespace AspNetCoreSpa.Core.Entities
{
    public class ChatRoomPhoto : PhotoBase
    {
        public string ChatRoomId { get; set; }

        public Room ChatRoom { get; set; }
    }
}
