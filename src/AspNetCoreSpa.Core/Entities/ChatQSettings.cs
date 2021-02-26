using System.ComponentModel.DataAnnotations;

namespace AspNetCoreSpa.Core.Entities
{
    public class ChatQSettings
    {
        [Key]
        public int Id { get; set; }

        public int MoneyPerHour { get; set; }

        public int MoneyServiceDelay { get; set; }
    }
}