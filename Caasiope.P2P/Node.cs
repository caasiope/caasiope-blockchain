using System.Net;
using System.Net.Sockets;

namespace Caasiope.P2P
{
    public class Node
    {
        public IPEndPoint EndPoint { get; private set; }
        public bool? IsPrivateEndPoint { get; private set; }
        public bool HasServer => EndPoint != null;

        public Node(IPEndPoint endPoint)
        {
            SetEndPoint(endPoint);
        }

        public Node(IPAddress ip, int port)
        {
            SetEndPoint(new IPEndPoint(ip, port));
        }

        public void SetEndPoint(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            IsPrivateEndPoint = endPoint?.Address.IsPrivate();
        }
    }

    public static class IpAddressExtensions
    {
        private static readonly IPAddressRange rangeChecker1 = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.255.255.255"));
        private static readonly IPAddressRange rangeChecker2 = new IPAddressRange(IPAddress.Parse("172.16.0.0"), IPAddress.Parse("172.32.255.255"));
        private static readonly IPAddressRange rangeChecker3 = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

        public static bool IsPrivate(this IPAddress address)
        {
            return rangeChecker1.IsInRange(address) || rangeChecker2.IsInRange(address) || rangeChecker3.IsInRange(address);
        }

        private class IPAddressRange
        {
            readonly AddressFamily addressFamily;
            readonly byte[] lowerBytes;
            readonly byte[] upperBytes;

            public IPAddressRange(IPAddress lowerInclusive, IPAddress upperInclusive)
            {
                // Assert that lower.AddressFamily == upper.AddressFamily

                addressFamily = lowerInclusive.AddressFamily;
                lowerBytes = lowerInclusive.GetAddressBytes();
                upperBytes = upperInclusive.GetAddressBytes();
            }

            // TODO maybe could check faster
            public bool IsInRange(IPAddress address)
            {
                if (address.AddressFamily != addressFamily)
                {
                    return false;
                }

                byte[] addressBytes = address.GetAddressBytes();

                bool lowerBoundary = true, upperBoundary = true;

                for (int i = 0; i < this.lowerBytes.Length && (lowerBoundary || upperBoundary); i++)
                {
                    if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                        (upperBoundary && addressBytes[i] > upperBytes[i]))
                    {
                        return false;
                    }

                    lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                    upperBoundary &= (addressBytes[i] == upperBytes[i]);
                }

                return true;
            }
        }

    }
}