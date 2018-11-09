using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Helios.Common.Extensions;

namespace Caasiope.P2P
{
    class DiscoveryProtocol
    {
        public Action<PeerSession, int> OnServerPort;
        public Action<PeerSession, List<IPEndPoint>> OnEndPointTable;
        public Action<PeerSession> OnPing;
        public Action<PeerSession> OnPong;

        enum MessageType
        {
            Ping,
            Pong,
            ServerPort,
            EndPointTable
        }
        
        public void HandleMessage(PeerSession peer, byte[] message)
        {
            using (var stream = new MemoryStream(message))
            {
                switch (stream.ReadByte())
                {
                    // get the server endpoint of the peer
                    case (int)MessageType.ServerPort:
                        var port = ReadInt(stream);
                        if (port <= 0) return;
                        OnServerPort.Call(peer, port);
                        break;
                    // get the server endpoints known by the peer
                    case (int)MessageType.EndPointTable:
                        var endpoints = ReadEndPointTable(stream);
                        OnEndPointTable.Call(peer, endpoints);
                        break;
                    case (int)MessageType.Ping:
                        OnPing.Call(peer);
                        break;
                    case (int)MessageType.Pong:
                        OnPong.Call(peer);
                        break;
                    default:
                        throw new ProtocolViolationException("Discovery Protocol doesnt know this message type");
                }
            }
        }

        private IPEndPoint ReadEndPoint(MemoryStream stream)
        {
            var address = ReadLong(stream);
            var port = ReadInt(stream);
            return new IPEndPoint(address, port);
        }

        private List<IPEndPoint> ReadEndPointTable(MemoryStream stream)
        {
            return ReadList(stream, ReadEndPoint);
        }

        private int ReadInt(MemoryStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        private long ReadLong(MemoryStream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }

        private void WriteEndPoint(MemoryStream stream, IPEndPoint endpoint)
        {
            WriteLong(stream, endpoint.Address.Address);
            WriteInt(stream, endpoint.Port);
        }

        private void WriteInt(MemoryStream stream, int data)
        {
            stream.Write(BitConverter.GetBytes(data), 0, 4);
        }

        private void WriteLong(MemoryStream stream, long data)
        {
            stream.Write(BitConverter.GetBytes(data), 0, 8);
        }

        private void WriteList<T>(MemoryStream stream, List<T> items, Action<MemoryStream, T> write)
        {
            if (items.Count > byte.MaxValue)
                throw new ArgumentException("too much items");
            stream.WriteByte((byte)items.Count);
            foreach (var item in items)
                write(stream, item);
        }

        private List<T> ReadList<T>(MemoryStream stream, Func<MemoryStream, T> read)
        {
            var length = stream.ReadByte();
            var list = new List<T>(length);
            for (int i = 0; i < length; i++)
                list.Add(read(stream));
            return list;
        }

        public byte[] ServerPort(int port)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte((byte)MessageType.ServerPort);
                WriteInt(stream, port);
                return stream.GetBuffer();
            }
        }

        public byte[] EndPointTable(List<IPEndPoint> endpoints)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte((byte)MessageType.EndPointTable);
                WriteList(stream, endpoints, WriteEndPoint);
                return stream.GetBuffer();
            }
        }

        public byte[] Ping()
        {
            return new []{(byte) MessageType.Ping};
        }

        public byte[] Pong()
        {
            return new []{(byte) MessageType.Pong};
        }
    }
}
