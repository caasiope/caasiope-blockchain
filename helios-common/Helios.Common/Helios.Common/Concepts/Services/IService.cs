using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Common.Concepts.Services
{
	public interface IService : IDisposable
	{
		string Name { get; }
		bool IsRunning { get; }

		Task Initialize();
		Task Start();
		Task Stop();

		Action Initialized { get; set; }
		Action Started { get; set; }
		Action Stopped { get; set; }

		WaitHandle InitializedHandle { get; }
		WaitHandle StartedHandle { get; }
		WaitHandle StoppedHandle { get; }
	}
}