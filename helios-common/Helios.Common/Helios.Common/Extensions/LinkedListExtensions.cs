using System.Collections.Generic;

namespace Helios.Common.Extensions
{
	public static class LinkedListExtensions
	{
		public static bool TryDequeue<T>(this LinkedList<T> list, out T value) where T : class
		{
			var node = list.First;
			if (node == null)
			{
				value = null;
				return false;
			}
			list.RemoveFirst();
			value = node.Value;
			return true;
		}

		public static void Enqueue<T>(this LinkedList<T> list, T value)
		{
			list.AddLast(value);
		}
	}
}
