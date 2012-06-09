using System.Runtime.Serialization;
using zpd;

namespace ZpdContract
{
    [DataContract]
    public class AuthPacket
    {
        [DataMember]
        public string AuthTolken { get; set; }

        [DataMember]
        public int Offset { get; set; }

        [DataMember]
        public AuthTolkenTimeout Timeout { get; set; }

        [DataMember]
        public int ClientId { get; set; }
    }
}