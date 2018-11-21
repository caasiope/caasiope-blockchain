using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Explorer.Database.Repositories
{
    public class TransactionDeclarationRepository : Repository<TransactionDeclarationEntity, transactiondeclaration>
	{
		private readonly DeclarationIndex transactions = new DeclarationIndex();
	    private long last;

	    private class DeclarationIndex : PrimaryIndex
		{
			private class Enumerator : IEnumerator<TransactionDeclarationEntity>
			{
				private readonly Dictionary<TransactionHash, List<Wrapper>> cache;
				private Dictionary<TransactionHash, List<Wrapper>>.Enumerator dictionnary;
				private List<Wrapper>.Enumerator list;

				public Enumerator(Dictionary<TransactionHash, List<Wrapper>> cache)
				{
					this.cache = cache;
					dictionnary = this.cache.GetEnumerator();
				}

				public void Dispose()
				{
					dictionnary.Dispose();
					list.Dispose();
				}

				public bool MoveNext()
				{
					if (list.Current == null)
					{
						if (!dictionnary.MoveNext())
							return false;

						list = dictionnary.Current.Value.GetEnumerator();
					}
					return MoveNextRecursive();
				}

				private bool MoveNextRecursive()
				{
					if (list.MoveNext())
						return true;

					return MoveNext();
				}

				public void Reset()
				{
					throw new NotImplementedException();
				}

				public TransactionDeclarationEntity Current => list.Current.Item;

			    object IEnumerator.Current => Current;
			}

			private readonly Dictionary<TransactionHash, List<Wrapper>> cache = new Dictionary<TransactionHash, List<Wrapper>>();

			protected internal override void Add(Wrapper item)
			{
				cache.GetOrCreate(item.Item.TransactionHash).Add(item);
			}

			protected internal override bool GetOrCreate(TransactionDeclarationEntity item, out Wrapper wrapper)
			{
				var list = cache.GetOrCreate(item.TransactionHash);
				wrapper = new Wrapper(item);
				list.Add(wrapper);
				return false;
			}

			public override IEnumerator<TransactionDeclarationEntity> GetEnumerator()
			{
				return new Enumerator(cache);
			}

			public IEnumerator<TransactionDeclarationEntity> GetEnumerator(TransactionHash hash)
			{
				List<Wrapper> list;
				if (cache.TryGetValue(hash, out list))
				{
					return new UnwrapEnumerator(list.GetEnumerator());
				}
				return new List<TransactionDeclarationEntity>().GetEnumerator();
			}
		}

		protected override transactiondeclaration ToEntity(TransactionDeclarationEntity item)
		{
			return new transactiondeclaration
			{
				transaction_hash = item.TransactionHash.Bytes,
				index = (byte)item.Index,
			    declaration_id = item.DeclarationId,
            };
		}

		protected override TransactionDeclarationEntity ToItem(transactiondeclaration entity)
		{
			return new TransactionDeclarationEntity(new TransactionHash(entity.transaction_hash), entity.index, entity.declaration_id);
		}

		protected override DbSet<transactiondeclaration> GetDbSet(BlockchainEntities entities)
		{
			return entities.transactiondeclarations;
		}

		protected override PrimaryIndex GetPrimaryIndex()
		{
			return transactions;
		}

		public IEnumerable<TransactionDeclarationEntity> GetEnumerable(TransactionHash hash)
		{
			return new EnumeratorToEnumerable<TransactionDeclarationEntity>(transactions.GetEnumerator(hash)).OrderBy(declaration => declaration.Index);
		}

	    public override void Initialize(BlockchainEntities entities)
	    {
	        base.Initialize(entities);
	        last = GetEnumerable().Select(declaration => declaration.DeclarationId).DefaultIfEmpty(-1).Max();
	    }

	    public long GetNextId()
	    {
	        return ++last;
	    }
	}
}
