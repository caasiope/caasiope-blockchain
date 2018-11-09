using System;

namespace Helios.Common.Concepts.Chronicles
{
	public abstract class Bard<TFolklore, TSaga> : Saga<TFolklore>.Bard, IDisposable where TSaga : Saga<TFolklore>
	{
		private readonly TFolklore folklore;
		public TSaga Saga { get; private set; }

		protected Bard(TFolklore folklore, TSaga saga)
		{
			this.folklore = folklore;
			Saga = saga;
		}

		public void Dispose()
		{
			foreach (var tale in GetTales(Saga))
				Terminate(tale, folklore);
			Terminate(Saga, folklore);
		}
	}
}