using System.Collections.Generic;

namespace Helios.Common.Concepts.Chronicles
{
	public abstract class Saga<TFolklore> : Chronicle<TFolklore>
	{
		public new abstract class Bard : Chronicle<TFolklore>.Bard
		{
			protected List<Tale<TFolklore>> GetTales(Saga<TFolklore> saga)
			{
				return saga.tales;
			}
		}
		protected List<Tale<TFolklore>> tales = new List<Tale<TFolklore>>();

		protected TTale RegisterTale<TTale>(TTale tale) where TTale : Tale<TFolklore>
		{
			tales.Add(tale);
			return tale;
		}
	}
}
