using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Database.Repositories
{
    public class TransactionInputOutputRepository : Repository<TxInputOutputFull, transactioninputoutput>
    {
	    private class AddressIndex : Index
		{
			readonly Dictionary<Address, List<Wrapper>> cache = new Dictionary<Address, List<Wrapper>>();

			protected internal override void Add(Wrapper item)
			{
				cache.GetOrCreate(item.Item.TxInputOutput.Address).Add(item);
			}

			public List<Wrapper> GetByAddress(Address address)
			{
				List<Wrapper> list;
				if(!cache.TryGetValue(address, out list))
					return new List<Wrapper>();
				return list;
			}
		}

	    private class TransactionIndex : PrimaryIndex
        {
            private class Enumerator : IEnumerator<TxInputOutputFull>
            {
                private readonly Dictionary<TransactionHash, List<Wrapper>> cache;
                Dictionary<TransactionHash, List<Wrapper>>.Enumerator dictionnary;
                private List<Wrapper>.Enumerator list;

                public Enumerator(Dictionary<TransactionHash, List<Wrapper>> cache)
                {
                    this.cache = cache;
                    dictionnary = cache.GetEnumerator();
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

                    list.Dispose();
                    if (!dictionnary.MoveNext())
                        return false;

                    list = dictionnary.Current.Value.GetEnumerator();
                    list.MoveNext();
                    return MoveNext();
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }

                public TxInputOutputFull Current => list.Current.Item;
                object IEnumerator.Current => Current;
            }

            private readonly Dictionary<TransactionHash, List<Wrapper>> cache = new Dictionary<TransactionHash, List<Wrapper>>();

            protected internal override void Add(Wrapper item)
            {
                cache.GetOrCreate(item.Item.TransactionHash).Add(item);
            }

            protected internal override bool GetOrCreate(TxInputOutputFull item, out Wrapper wrapper)
            {
                var list = cache.GetOrCreate(item.TransactionHash);
                // we assert that we dont update
                Debug.Assert(list.All(t => t.Item.Index != item.Index));

                wrapper = new Wrapper(item);
                list.Add(wrapper);
                return false;
            }

            public override IEnumerator<TxInputOutputFull> GetEnumerator()
            {
                return new Enumerator(cache);
            }

            public IEnumerator<TxInputOutputFull> GetEnumerator(TransactionHash hash)
            {
                List<Wrapper> list;
                if (cache.TryGetValue(hash, out list))
                {
                    return new UnwrapEnumerator(list.GetEnumerator());
                }
                return new List<TxInputOutputFull>().GetEnumerator();
            }
        }

        private readonly TransactionIndex transactions = new TransactionIndex();
		// TODO use only for explorer
        private readonly AddressIndex addresses = new AddressIndex();

	    public TransactionInputOutputRepository()
	    {
		    RegisterIndex(addresses);
	    }

		protected override DbSet<transactioninputoutput> GetDbSet(BlockchainEntities entities)
        {
            return entities.transactioninputoutputs;
        }

        protected override transactioninputoutput ToEntity(TxInputOutputFull item)
        {
            var isInput = item.TxInputOutput is TxInput;
            return new transactioninputoutput()
            {
                index = item.Index,
                is_input = isInput,
                account = item.TxInputOutput.Address.ToRawBytes(),
                currency = item.TxInputOutput.Currency,
                amount = item.TxInputOutput.Amount,
                transaction_hash = item.TransactionHash.Bytes,
            };
        }

        protected override TxInputOutputFull ToItem(transactioninputoutput entity)
        {
            var inputOutput = entity.is_input ?
                                 new TxInput(Address.FromRawBytes(entity.account), entity.currency, Amount.FromRaw(entity.amount))
                : (TxInputOutput)new TxOutput(Address.FromRawBytes(entity.account), entity.currency, Amount.FromRaw(entity.amount));

            return new TxInputOutputFull(inputOutput, new TransactionHash(entity.transaction_hash), entity.index);
        }

        protected override PrimaryIndex GetPrimaryIndex()
        {
            return transactions;
        }

        public IEnumerable<TxInputOutputFull> GetEnumerable(TransactionHash hash)
        {
            return new EnumeratorToEnumerable<TxInputOutputFull>(transactions.GetEnumerator(hash));
        }

	    public IEnumerable<TxInputOutputFull> GetByAddress(Address address)
		{
			return new EnumeratorToEnumerable<TxInputOutputFull>(new UnwrapEnumerator(addresses.GetByAddress(address).GetEnumerator()));
		}
    }
}