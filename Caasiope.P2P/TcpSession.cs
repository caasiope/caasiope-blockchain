using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Helios.Common.Logs;

namespace Caasiope.P2P
{
    internal class TcpSession
    {
        private readonly TcpClient client;
        private readonly X509Certificate2 certificate;
        private SslStream sslStream;
        private readonly Action<SslStream> authenticate;
        private BinaryReader reader;
        public Persona Persona { get; private set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        public TcpSession(TcpClient client, X509Certificate2 certificate, Action<SslStream> authenticate, ILogger logger, bool isServer)
        {
            this.client = client;
            this.certificate = certificate;
            this.authenticate = authenticate;
        }

        private bool ValidateCertificateCallBack(object sender, X509Certificate x509Certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            /*
            if (!new X509Certificate2(x509Certificate).Verify())
                return false;
                */
            Persona = new Persona(new X509Certificate2(x509Certificate));
            return true;
        }

        public bool Write(byte channel, byte[] data)
        {
            try
            {
                var buffer = new byte[data.Length + 4 + 1];
                buffer[0] = channel;
                Array.Copy(BitConverter.GetBytes(data.Length), 0, buffer, 1, 4);
                Array.Copy(data, 0, buffer, 4 + 1, data.Length);
                sslStream.Write(buffer, 0, buffer.Length);
                sslStream.Flush();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public void AuthenticateAsServer(SslStream sslStream)
        {
            sslStream.AuthenticateAsServer(certificate, true, SslProtocols.Tls, false);
        }

        public void AuthenticateAsClient(SslStream sslStream, string host)
        {
            sslStream.AuthenticateAsClient(host, new X509Certificate2Collection(certificate), SslProtocols.Tls, false);
        }

        // private bool isClosed;
        /*
        public void Close()
        {

                sslStream?.Close();
                // if (!isClosed)
                    OnClose.Call(); // TODO call only one time

                // isClosed = true;
        }
        */

        public bool Authenticate()
        {
            sslStream = new SslStream(client.GetStream(), false, ValidateCertificateCallBack);
            // send authentication
            authenticate(sslStream);
            if (!sslStream.IsMutuallyAuthenticated && !sslStream.IsEncrypted)
            {
                return false;
            }

            reader = new BinaryReader(sslStream);
            LocalEndPoint = (IPEndPoint) client.Client.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint) client.Client.RemoteEndPoint;
            return true;
        }

        public bool Read(ref byte channel, ref byte[] buffer)
        {
            var channelTmp = new byte[1];
            if (reader.Read(channelTmp, 0, 1) != 1)
                return false;
            channel = channelTmp[0];
            // throw new DataMisalignedException("Cannot read Channel");

            var lenght = new byte[4];
            if (reader.Read(lenght, 0, 4) != 4)
                return false;
            // throw new DataMisalignedException("Cannot read Lenght");

            var expected = BitConverter.ToInt32(lenght, 0);
            // we have to use BinaryReader or while loop because SSL stream frame if 16 kb
            buffer = reader.ReadBytes(expected);
            if (buffer.Length != expected)
                return false;
            // throw new DataMisalignedException($"Cannot read Message. Expected {expected} readed {buffer.Length}");
            return true;
        }

        public void Close()
        {
            client.Close();
            sslStream?.Dispose();
            reader?.Dispose();
        }
    }
}