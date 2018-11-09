using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Common.Logs;

namespace Caasiope.P2P
{
    internal class Server
    {
        private ILogger logger;
        private readonly P2PConfiguration configuration;
        private readonly TcpListener listener;
        public Action<NodeSession> OnClientAuthenticated;
        private TcpSession tcp;

        public Server(P2PConfiguration configuration)
        {
            this.configuration = configuration;
            listener = new TcpListener(configuration.IPEndpoint);
        }

        public void Start()
        {
            listener.Start();
            Task.Run(new Action(ServerListenLoop));
        }

        private void ServerListenLoop()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                tcp = new TcpSession(client, configuration.Certificate, Authenticate, logger, true);
                var session = new NodeSession(tcp, logger);
                session.OnAuthenticated += (local, remote) => OnClientAuthenticated(session);
                // TODO handle connection state changed
                session.Run();
            }
        }

        private void Authenticate(SslStream sslStream)
        {
            tcp.AuthenticateAsServer(sslStream);
        }

        public void Initialize(ILogger logger)
        {
            this.logger = logger;
        }
    }
}