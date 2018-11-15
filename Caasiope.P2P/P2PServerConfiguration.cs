using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Helios.Common.Configurations;

namespace Caasiope.P2P
{
    public class P2PServerConfiguration
    {
        public static P2PConfiguration LoadConfiguration(string path, string defaultPath)
        {
            var lines = new DictionaryConfiguration(path);

            X509Certificate2 cert = null;
            IPEndPoint endpoint = null;
            try
            {
                var certPath = lines.GetValue("TLS_CERT");
                var pwd = lines.GetValue("TLS_PWD");
                if (File.Exists(certPath))
                    cert = new X509Certificate2(certPath, pwd);
            }
            catch (Exception e)
            {
            }

            if (cert == null)
            {
                if (File.Exists(defaultPath))
                    cert = new X509Certificate2(defaultPath, "");
            }

            try
            {
                var ip = IPAddress.Any;
                if(!string.IsNullOrEmpty(lines.GetValue("Ip")))
                    ip = IPAddress.Parse(lines.GetValue("Ip"));
                endpoint = new IPEndPoint(ip, int.Parse(lines.GetValue("Port")));
            }
            catch (Exception e) { }

            int forwarded = 0;
            try
            {
                if (!string.IsNullOrEmpty(lines.GetValue("ForwardedPort")))
                    forwarded = Int32.Parse(lines.GetValue("ForwardedPort"));
            }
            catch (Exception e) { }

            return new P2PConfiguration
            {
                IPEndpoint = endpoint,
                Certificate = cert,
                ForwardedPort = forwarded
            };
        }

        public static List<IPEndPoint> ToIPEndpoints(IReadOnlyCollection<string> addresses)
        {
            var results = new List<IPEndPoint>();
            foreach (var address in addresses)
            {
                results.Add(NodeStorage.ParseIPEndPoint(address));
            }

            return results;
        }
    }
}