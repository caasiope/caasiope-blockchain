using System;
using System.IO;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;

namespace Caasiope.Node
{
    public static class InitialLedgerConfiguration
    {
        public static SignedLedger LoadLedger(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    var bytes = File.ReadAllBytes(path);
                    using (var stream = new ByteStream(bytes))
                    {
                        return stream.ReadSignedLedger();
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            throw new FileNotFoundException(path);
        }

        public static void SaveLedger(SignedLedger signed, string path)
        {
            using (var stream = new ByteStream())
            {
                stream.Write(signed);
                var bytes = stream.GetBytes();
                File.WriteAllBytes(path, bytes);
            }
        }

        public static string GetDefaultPath()
        {
            return NodeConfiguration.GetPath("initial.block");
        }
    }
}