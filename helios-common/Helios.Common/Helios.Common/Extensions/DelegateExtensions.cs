using System;
using System.Diagnostics;

namespace Helios.Common.Extensions
{
	public static class DelegateExtensions
	{
		[DebuggerStepThrough]
		public static void Call<T>(this EventHandler<T> handler, object sender, T arg) where T : EventArgs
		{
			if (handler != null) handler(sender, arg);
		}

		[DebuggerStepThrough]
		public static void Call<T>(this Action<T> handler, T arg)
		{
			if (handler != null) handler(arg);
		}

		[DebuggerStepThrough]
		public static void Call<T1,T2>(this Action<T1,T2> handler, T1 arg1, T2 arg2)
		{
			if (handler != null) handler(arg1,arg2);
		}

		[DebuggerStepThrough]
		public static void Call<T1,T2,T3>(this Action<T1,T2,T3> handler, T1 arg1, T2 arg2, T3 arg3)
		{
			if (handler != null) handler(arg1,arg2,arg3);
		}

		[DebuggerStepThrough]
		public static void Call<T1,T2,T3,T4>(this Action<T1,T2,T3,T4> handler, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (handler != null) handler(arg1, arg2, arg3, arg4);
		}

		[DebuggerStepThrough]
		public static void Call(this Action handler)
		{
			if (handler != null) handler();
		}
	}
}
