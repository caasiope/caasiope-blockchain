using System.IO;
using System.Net;
using Caasiope.P2P.Security;

namespace Caasiope.P2P
{
    public class NodeBuilder
    {
        // TODO be able to add password to pem file
        public static P2PConfiguration BuildConfiguration(string path_server, string certPath, string defaultCertName)
        {
            var defaultCertPath = Path.Combine(certPath, defaultCertName);
            var configuration = P2PServerConfiguration.LoadConfiguration(path_server, certPath, defaultCertPath);
            // initialize fields if not set
            if (configuration.Certificate == null)
            {
                var certificate = CertificateHelper.GenerateCertificate(defaultCertPath);
                configuration.Certificate = certificate;
            }

            if (configuration.IPEndpoint == null)
            {
                configuration.IPEndpoint = new IPEndPoint(IPAddress.Any, 2020);
            }

            return configuration;
        }
    }
}
