using System.Threading;

namespace Helios.Common.Concepts.CQRS
{
	public interface ICommand
	{
		void Process();
	}

	public abstract class Command<T> : CommandProcessor.Command
	{
		private readonly T data;
		private readonly ManualResetEventSlim finished = new ManualResetEventSlim(false);

		protected Command(T data)
		{
			this.data = data;
		}

		// must be called by processing thread
		protected override void Process()
		{
			try
			{
				DoWork(data);
			}
			finally
			{
				finished.Set();
			}
		}

		// must be called by external thread
		public T GetOutput()
		{
			finished.Wait();
			return data;
		}

		protected abstract void DoWork(T input);
	}
}
