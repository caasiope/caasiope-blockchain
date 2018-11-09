using System;
using Caasiope.Protocol;

namespace Caasiope.JSON.Helpers
{
	public static class ByteStreamConverter
    {
	    public static string ToBase64<T>(Action<T> write) where T : ByteStream, new()
		{
            byte[] bytes;
            using (var stream = new T())
            {
                write(stream);
                bytes = stream.GetBytes();
            }
            return Convert.ToBase64String(bytes);
        }
    }
}