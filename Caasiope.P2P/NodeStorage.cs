using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Helios.Common.Logs;

namespace Caasiope.P2P
{
    internal class NodeStorage
    {
        private readonly ILogger logger;
        private string storagePath = "IPEndpoints.txt";

        public NodeStorage(ILogger logger)
        {
            this.logger = logger;
        }

        public List<IPEndPoint> LoadEndpoints()
        {
            var lines = new List<string>();

            if (File.Exists(storagePath))
                lines = File.ReadAllLines(storagePath).ToList();

            var endpoints = new List<IPEndPoint>();
            foreach (var line in lines)
            {
                try
                {
                    endpoints.Add(ParseIPEndPoint(line));
                }
                catch (Exception e)
                {
                    logger.Log("NodeStorage.LoadNodes()", e);
                }
            }

            return endpoints;
        }

        private PersonaThumbprint CreatePeerId(string thumbprint)
        {
            return new PersonaThumbprint(thumbprint);
        }

        // Handles IPv4 and IPv6 notation.
        public static IPEndPoint ParseIPEndPoint(string endPoint)
        {
            var ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }

            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.InvariantInfo, out var port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public void SaveNodes(List<Node> nodes)
        {
            var toSave = nodes.Where(_ => _.HasServer).Select(_ => _.EndPoint.ToString());

            try
            {
                File.WriteAllLines(storagePath, toSave);
            }
            catch (Exception e)
            {
                logger.Log("NodeStorage.SaveNodes()", e);
            }
        }

        public void WipeNodes()
        {
            try
            {
                File.Delete(storagePath);
            }
            catch (Exception e)
            {
                logger.Log("NodeStorage.WipeStorage(), storage is not wiped", e);
            }
        }
    }
}