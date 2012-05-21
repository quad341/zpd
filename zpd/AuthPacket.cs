namespace zpd
{
    public class AuthPacket
    {
        public string AuthTolken { get; set; }
        public int Offset { get; set; }
        public AuthTolkenTimeout Timeout { get; set; }
    }
}