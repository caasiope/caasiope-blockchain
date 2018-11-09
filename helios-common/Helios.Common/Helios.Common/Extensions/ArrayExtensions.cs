using System;

namespace Helios.Common.Extensions
{
	public static class ArrayExtensions
	{
		public static T[] Copy<T>(this T[] source)
		{
			var array = new T[source.Length];
			source.CopyTo(array, 0);
			return array;
		}

        public static byte[] SubArray(this byte[] data, int index, int length)
        {
            var bytes = new byte[length];
            Array.Copy(data, index, bytes, 0, length);
            return bytes;
        }

        public static byte[] Append(this byte[] array, byte[] tail)
        {
            var headerLength = array.Length;
            var bodyLength = tail.Length;
            var bytes = new byte[bodyLength + headerLength];
            Array.Copy(array, 0, bytes, 0, headerLength);
            Array.Copy(tail, 0, bytes, headerLength, bodyLength);
            return bytes;
        }

        public static bool IsEqual(this byte[] a, byte[] b)
        {
            if (a == b)
            {
                return true;
            }
            if ((a != null) && (b != null))
            {
                if (a.Length != b.Length)
                {
                    return false;
                }
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static string ToHexString(this byte[] array)
        {
            var sbinary = "";

            for (var i = 0; i < array.Length; i++)
            {
                sbinary += array[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }
    }
}
