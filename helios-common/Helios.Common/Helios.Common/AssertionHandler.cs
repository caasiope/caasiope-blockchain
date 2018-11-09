using System;
using System.Diagnostics;

namespace Helios.Common
{
	internal class AssertionFailedException : Exception
	{
		public AssertionFailedException(string message)
			: base(message)
		{
		}
	}


	public class ExceptionThrowListener : TraceListener
	{
		[DebuggerStepThrough]
		public override void Write(string message)
		{
			var exception = new AssertionFailedException(message);
			throw exception;
		}

		[DebuggerStepThrough]
		public override void WriteLine(string message)
		{
			var exception = new AssertionFailedException(message);
			throw exception;
		}
	}

	public static class AssertionHandler
	{
		public static void CatchAssertions()
		{
			Debug.Listeners.Clear();
			Debug.Listeners.Add(new ExceptionThrowListener());
		}
	}
}
