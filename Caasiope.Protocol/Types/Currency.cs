using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Caasiope.Protocol.Types
{
    [DebuggerDisplay("{Currency.ToSymbol(this)}")]
    public class Currency
    {
        private const int Offset = (short)'A';
        private const int Max = 17575;
        
        public static readonly Currency BTC = FromSymbol("BTC");
        public static readonly Currency LTC = FromSymbol("LTC");
        public static readonly Currency ETH = FromSymbol("ETH");
        public static readonly Currency CAS = FromSymbol("CAS");

        private readonly short value;

        private Currency(short value)
        {
            this.value = value;
        }

        public static Currency FromSymbol(string symbol)
        {
            Debug.Assert(symbol.Length == 3);

            int buffer = 0;
            int multiplier = 1;
            for (int i = 0; i < 3; i++)
            {
                buffer += ToValue(symbol[2 - i]) * multiplier;
                multiplier *= 26;
            }

            return (short) buffer;
        }

        public static string ToSymbol(Currency currency)
        {
            Debug.Assert(currency <= Max);
            int buffer = currency;
            var array = new char[3];
            for (int i = 0; i < 3; i++)
            {
                var rest = buffer%26;
                array[2 - i] = FromValue(rest);
                buffer /= 26;
            }
            return new string(array);
        }

        private static int ToValue(char c)
        {
            return c - Offset;
        }

        private static char FromValue(int c)
        {
            return (char) (c + Offset);
        }

        public static implicit operator Currency(short value)
        {
            return new Currency(value);
        }

        public static implicit operator short(Currency currency)
        {
            return currency.value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var currency = obj as Currency;
            return currency != null && value == currency.value;
        }

        public static bool operator == (Currency a, Currency b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.value == b.value;
        }

        public static bool operator != (Currency a, Currency b)
        {
            return !(a == b);
        }

        public static int Compare(Currency x, Currency y)
        {
            if (x.value == y.value)
                return 0;
            else if (x.value > y.value)
                return 1;
            return -1;
        }
    }

    public class CurrencyComparer : IComparer<Currency>
    {
        public int Compare(Currency x, Currency y)
        {
            return Currency.Compare(x, y);
        }
    }

    // version < cip#0001 : immutable state
    public class CurrencyComparer1 : IComparer<Currency>
    {
        public int Compare(Currency x, Currency y)
        {
            if (x.GetHashCode() == y.GetHashCode())
                return 0;
            else if (x.GetHashCode() > y.GetHashCode())
                return 1;
            return -1;
        }
    }
}