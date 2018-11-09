using System.IO;

namespace Caasiope.NBitcoin.BouncyCastle.asn1
{
	internal interface Asn1OctetStringParser
		: IAsn1Convertible
	{
		Stream GetOctetStream();
	}
}
