using System;
using System.Threading;
using Helios.Common.Extensions;

namespace Helios.Common.Synchronization
{
	public class MonitorLocker
	{
		public IDisposable CreateLock()
		{
			return new PrivateLock(this);
		}
	}

	public class Lock : IDisposable
	{
		private readonly PrivateLock locker;

		public Lock(object locker)
		{
			this.locker = new PrivateLock(locker);
		}

		public void Dispose()
		{
			((IDisposable)locker).Dispose();
		}
	}

	public class PrivateLock : IDisposable
	{
		public static TimeSpan DeadLockTimeout;
		public static Action OnDeadLock;
		private readonly object locker;
		private readonly bool lockTaken;

		static PrivateLock()
		{
			DeadLockTimeout = TimeSpan.FromSeconds(30);
		}

		internal PrivateLock(object locker)
		{
			this.locker = locker;
			// try to acquire lock
			Monitor.TryEnter(locker, DeadLockTimeout, ref lockTaken);
			if (lockTaken) return;
			// timeout maybe dead lock
			OnDeadLock.Call();
			// acquire the lock and wait for a human to kill the application
			Monitor.Enter(locker, ref lockTaken);
		}

	    void IDisposable.Dispose()
		{
			if (lockTaken)
			{
				Monitor.Exit(locker);
			}
		}
    }
}
