using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Helios.Common.Extensions;

namespace Helios.Common.Concepts.Services
{
	public abstract class ThreadedService : Service
	{
		private WaitHandle[] handles = new WaitHandle[0];
		private List<Action> callbacks = new List<Action>();
		private Thread thread;
		protected readonly AutoResetEvent trigger = new AutoResetEvent(false);
		private bool isTerminated;

		protected ThreadedService(string name = null) : base(name)
		{
			RegisterWaitHandle(trigger);
			Initialized += () => { thread = new Thread(ThreadLoop); thread.Start(); };
		}

		private void ThreadLoop()
		{
			Thread.CurrentThread.Name = Name;
			while (!isTerminated)
			{
				try
				{
					WaitEvent();
				}
				catch (Exception e)
				{
					Logger.Log("ThreadLoop", e);
				}
			}
		}

		private void WaitEvent()
		{
			var index = WaitHandle.WaitAny(handles);
			callbacks[index].Call();
			if (IsRunning)
			{
				using (CreateRunScope(Name))
				{
					Run();
				}
			}
		}

		private static readonly IDisposable disposable = new Disposable();
		private class Disposable : IDisposable { public void Dispose() { } }

		public virtual IDisposable CreateRunScope(string name)
		{
			return disposable;
		}

		protected abstract void Run();

		protected void RegisterWaitHandle(WaitHandle handle, Action callback = null, bool isImportant = false)
		{
			List<WaitHandle> list = new List<WaitHandle>(handles.Length + 1);
			if (isImportant)
			{
				list.Add(handle);
				list.AddRange(handles);
				var c = new List<Action>(callbacks.Count + 1);
				c.Add(callback);
				c.AddRange(callbacks);
				callbacks = c;
			}
			else
			{
				list.AddRange(handles);
				list.Add(handle);
				callbacks.Add(callback);
			}
			handles = list.ToArray();
		}

		public sealed override void Dispose()
		{
			isTerminated = true;
			if (thread == null) return;
			trigger.Set();
			thread.Join();
		}
	}
}