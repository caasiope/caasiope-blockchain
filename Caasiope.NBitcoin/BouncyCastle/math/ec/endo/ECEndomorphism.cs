namespace Caasiope.NBitcoin.BouncyCastle.math.ec.endo
{
	internal interface ECEndomorphism
	{
		ECPointMap PointMap
		{
			get;
		}

		bool HasEfficientPointMap
		{
			get;
		}
	}
}
