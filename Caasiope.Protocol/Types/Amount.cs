using System;
using System.Diagnostics;

namespace Caasiope.Protocol.Types
{
    [DebuggerDisplay("{value}")]
    public class Amount
    {
        public const uint NB_DECIMALS = 100000000;

        private readonly long value;

        private Amount(long value)
        {
            this.value = value;
        }

        public static implicit operator Amount(long value)
        {
            return new Amount(value);
        }

        public static implicit operator long(Amount currency)
        {
            return currency.value;
        }

        public static Amount FromWholeValue(long i)
        {
            // TODO check for max value
            return new Amount(i * NB_DECIMALS);
        }

        public static Amount FromWholeDecimal(decimal i)
        {
            // TODO check for max value
            var val = Math.Truncate(i * NB_DECIMALS);
            Debug.Assert(val == i * NB_DECIMALS);
            return new Amount((long)val);
		}

	    public static decimal ToWholeDecimal(Amount amount)
	    {
			return new decimal(amount) / NB_DECIMALS;
	    }

		public static Amount FromRaw(long i)
        {
            // TODO check for max value
            return new Amount(i);
        }
    }
}