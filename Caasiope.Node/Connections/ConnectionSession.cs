using System.Diagnostics;
using Caasiope.P2P;

namespace Caasiope.Node.Connections
{
    public interface IConnectionSession
    {
        ISession Session { get; }
    }

    public class ConnectionSession : IConnectionSession
    {
        public ConnectionSession(ISession session)
        {
            Debug.Assert(session != null);
            Session = session;
        }

        public ISession Session { get; }
    }
}