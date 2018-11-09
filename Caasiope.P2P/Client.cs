using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Helios.Common.Logs;

namespace Caasiope.P2P
{
    internal class Client
    {
        private readonly TcpClient client;
        public readonly TcpSession TcpSession;
        public readonly NodeSession NodeSession;
        private readonly IPEndPoint endpoint;
        private readonly ILogger logger;

        public Client(IPEndPoint endpoint, X509Certificate2 certificate, ILogger logger)
        {
            this.logger = logger;
            this.endpoint = endpoint;
            client = new TcpClient();
            TcpSession = new TcpSession(client, certificate, Authenticate, logger, false);
            NodeSession = new NodeSession(TcpSession, logger);
        }

        private void Authenticate(SslStream sslStream)
        {
            TcpSession.AuthenticateAsClient(sslStream, endpoint.ToString());
        }

        public void Start()
        {
            Task.Run(new Action(Start_));
        }

        public void Start_()
        {
            var isError = false;
            try
            {
                client.Connect(endpoint);

                // we should create the node session here to mirror the server
                if (!NodeSession.Run())
                    return;
            }
            catch (SocketException socketException) // Host down
            {
                if (socketException.SocketErrorCode == SocketError.ConnectionRefused) { }
                if (socketException.SocketErrorCode == SocketError.HostDown) { }
                if (socketException.SocketErrorCode == SocketError.TimedOut) { }
                isError = true;
            }
            catch (Exception e)
            {
                isError = true;
            }

            if (isError)
            {
                // WTF ? why not close client
                NodeSession.Disconnect(DisconnectReason.CannotConnectToNode);
            }
        }
    }
}