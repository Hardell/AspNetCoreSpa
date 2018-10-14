using System.ComponentModel.DataAnnotations;

namespace AspNetCoreSpa.Core.Entities
{
    public class PhotoBase : IEntityBase
    {
        [Key]
        public int Id { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}
