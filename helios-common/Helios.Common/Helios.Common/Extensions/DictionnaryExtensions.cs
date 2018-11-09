using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helios.Common.Extensions
{
	public static class DictionnaryExtensions
	{
		[DebuggerStepThrough]
		public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
		{
			TValue value;
			if (dict.TryGetValue(key, out value)) return value;
			value = new TValue();
			dict.Add(key, value);
			return value;
		}

		[DebuggerStepThrough]
		public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> create)
		{
			TValue value;
			if (dict.TryGetValue(key, out value)) return value;
			value = create();
			dict.Add(key, value);
			return value;
		}

		[DebuggerStepThrough]
		public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> create)
		{
			TValue value;
			if (dict.TryGetValue(key, out value)) return value;
			value = create(key);
			dict.Add(key, value);
			return value;
		}
	}
}