using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Helios.Common.Logs;
using Caasiope.Node;
using Caasiope.Node.Connections;
using Caasiope.P2P;
using Caasiope.P2P.Security;
using IP2PConnection = Caasiope.Node.Connections.IP2PConnection;
using System.Net;

namespace Caasiope.UnitTest.FakeServices
{
	public class FakeP2PConnection : IP2PConnection, IChannelConnection
	{
	    private int channels;
		public void Start()
		{
			
		}

		public void Stop()
		{
		}

	    public IChannel CreateChannel(byte header)
	    {
	        return new Channel(header, this, channels++);
	    }

	    public void OnConnected(Action<ISession> callback)
	    {
	    }

	    public X509Certificate2 GetCertificate()
	    {
            return CertificateHelper.LoadCertificate(NodeConfiguration.GetPath("certificates/envy.pem")); ;
	    }

	    public void Initialize(ILogger logger)
	    {
	    }

	    public IEnumerable<IPeer> GetConnectedPeers()
	    {
	        throw new NotImplementedException();
	    }

	    public IEnumerable<P2P.Node> GetAllNodes()
	    {
	        throw new NotImplementedException();
	    }

	    public IConnectionSession FakeSession()
	    {
	        return new ConnectionSession(new PeerSession(null, 1)); 
	    }

	    public void Broadcast(IChannel channel, byte[] data)
	    {
	    }

	    public void Send(ISession session, IChannel channel, byte[] data)
	    {
	    }

        public IEnumerable<IPEndPoint> GetSelfEndPoints()
        {
            throw new NotImplementedException();
        }

	    public void WipeNodeList()
	    {
	        throw new NotImplementedException();
	    }
	}
}
