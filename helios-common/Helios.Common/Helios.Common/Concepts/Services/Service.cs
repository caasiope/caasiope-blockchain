using System;
using System.Threading;
using System.Threading.Tasks;
using Helios.Common.Extensions;
using Helios.Common.Logs;

namespace Helios.Common.Concepts.Services
{
    public abstract class Service : IService
	{
		public string Name { get; }

		public abstract ILogger Logger { get; }

		public bool IsRunning { get; private set; }

		public Action Initialized { get; set; }
		public Action Started { get; set; }
		public Action Stopped { get; set; }

		private readonly ManualResetEventSlim initialized = new ManualResetEventSlim();
		public WaitHandle InitializedHandle => initialized.WaitHandle;

	    private readonly ManualResetEventSlim started = new ManualResetEventSlim();
		public WaitHandle StartedHandle => started.WaitHandle;

	    private readonly ManualResetEventSlim stopped = new ManualResetEventSlim();
		public WaitHandle StoppedHandle => stopped.WaitHandle;

	    protected Service(string name = null)
		{
			Name = name ?? GetType().Name;
		}

		public Task Initialize()
		{
			return Task.Factory.StartNew(delegate
			{
				try
				{
					InitializeImpl();
				}
				catch (Exception e)
				{
					OnInitializeException(e);
				}
			});
		}

		protected virtual void InitializeImpl()
		{
			OnInitialize();
			Initialized.Call();
			initialized.Set();
		}

		protected virtual void OnInitializeException(Exception exception)
		{
			Logger.Log("OnInitializeException", exception);
		}

		protected abstract void OnInitialize();

		public Task Start()
		{
			return Task.Factory.StartNew(delegate
			{
				try
				{
					StartImpl();
				}
				catch (Exception e)
				{
					OnStartException(e);
				}
			});
		}

		protected virtual void StartImpl()
		{
			stopped.Reset();
			OnStart();
			IsRunning = true;
			Started.Call();
			started.Set();
		}

		protected virtual void OnStartException(Exception exception)
		{
			Logger.Log("OnStartException", exception);
		}

		protected abstract void OnStart();

		public Task Stop()
		{
			return Task.Factory.StartNew(delegate
			{
				try
				{
					StopImpl();
				}
				catch (Exception e)
				{
					OnStopException(e);
				}
			});
		}

		protected virtual void StopImpl()
		{
			started.Reset();
			OnStop();
			IsRunning = false;
			Stopped.Call();
			stopped.Set();
		}

		protected virtual void OnStopException(Exception exception)
		{
			Logger.Log("OnStopException", exception);
		}

		protected abstract void OnStop();

		public virtual void Dispose() { }
	}
}