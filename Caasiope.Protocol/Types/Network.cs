using System;
using System.Collections.Generic;
using System.Text;

namespace Caasiope.Protocol.Types
{
	public abstract class Network
	{
		public static class VersionByte
		{
			public static string SIGNED_MESSAGE_HEADER_BYTES = "SIGNED_MESSAGE_HEADER_BYTES";
			public const string BIP38_ENCRYPTED_PRIVATE_KEY = "BIP38_ENCRYPTED_PRIVATE_KEY";
		}

		protected Dictionary<string, byte[]> versionBytes = new Dictionary<string, byte[]>();
		public byte[] GetVersionBytes(string name)
		{
			return versionBytes[name];
		}

	    public readonly string Name;

	    protected Network(string name)
	    {
	        Name = name;
	    }
	}

	public class MainNetwork
    {
        public static Network Instance => OlympusNetwork.Instance;
    }

	public class TestNetwork
	{
	    public static Network Instance => ZodiacNetwork.Instance;
	}

	public class LocalNetwork
	{
	    public static Network Instance => ViceNetwork.Instance;
	}

	public class ViceNetwork : Network
	{
		public static ViceNetwork Instance = new ViceNetwork();

		private ViceNetwork() : base("vice")
		{
			versionBytes.Add(VersionByte.BIP38_ENCRYPTED_PRIVATE_KEY, new byte[]{ 0x76 });
			versionBytes.Add(VersionByte.SIGNED_MESSAGE_HEADER_BYTES, Encoding.UTF8.GetBytes($"{Name}.caasiope.net"));
		}
	}

	public class ZodiacNetwork : Network
	{
		public static ZodiacNetwork Instance = new ZodiacNetwork();

		private ZodiacNetwork() : base("zodiac")
		{
			versionBytes.Add(VersionByte.BIP38_ENCRYPTED_PRIVATE_KEY, new byte[]{ 0x66 });
		    versionBytes.Add(VersionByte.SIGNED_MESSAGE_HEADER_BYTES, Encoding.UTF8.GetBytes($"{Name}.caasiope.net"));
        }
    }

    public class OlympusNetwork : Network
    {
        public static OlympusNetwork Instance = new OlympusNetwork();

        private OlympusNetwork() : base("olympus")
        {
            versionBytes.Add(VersionByte.BIP38_ENCRYPTED_PRIVATE_KEY, new byte[] { 0x86 });
            versionBytes.Add(VersionByte.SIGNED_MESSAGE_HEADER_BYTES, Encoding.UTF8.GetBytes($"{Name}.caasiope.net"));
        }
    }
}