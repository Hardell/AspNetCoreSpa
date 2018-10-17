using System.Runtime.Serialization;

namespace AspNetCoreSpa.Web.Commands
{
    [DataContract]
    public class CommandResponse
    {
        [DataMember]
        public string Command { get; set; }

        [DataMember]
        public object Content { get; set; }
    }
}
