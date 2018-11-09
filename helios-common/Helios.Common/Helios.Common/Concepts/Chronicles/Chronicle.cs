namespace Helios.Common.Concepts.Chronicles
{
	public abstract class Chronicle<TFolklore>
	{
		public abstract class Bard
		{
			protected void Terminate(Chronicle<TFolklore> chronicle, TFolklore folklore)
			{
				chronicle.Terminate(folklore);
			}
		}

		protected abstract void Initialize(TFolklore folklore);
		protected abstract void Terminate(TFolklore folklore);
	}
}