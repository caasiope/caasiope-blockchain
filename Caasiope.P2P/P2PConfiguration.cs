using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Caasiope.P2P
{
    public class P2PConfiguration
    {
        public IPEndPoint IPEndpoint { get; set; }
        public X509Certificate2 Certificate { get; set; }
        public int ForwardedPort { get; set; }
    }
}