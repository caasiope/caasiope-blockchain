using System;
using System.Collections.Generic;

namespace Caasiope.Wallet.Managers
{
    public interface IAliased
    {
        string Alias { get; }
        T GetObject<T>() where T : class;
    }

    public class Aliased<T> : IAliased
    {
        public string Alias { get; }
        public readonly T Data;

        public Aliased(string alias, T data)
        {
            Alias = alias;
            Data = data;
        }
        
        public TObject GetObject<TObject>() where TObject : class
        {
            return Data as TObject;
        }
    }

    public class AliasManager
    {
        private readonly Dictionary<string, IAliased> aliases = new Dictionary<string, IAliased>();

        public Aliased<T> SetAlias<T>(string alias, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (String.IsNullOrEmpty(alias))
                throw new ArgumentNullException(nameof(alias));
            var aliased = new Aliased<T>(alias, item);
            aliases[alias] = aliased;
            return aliased;
        }

        public bool TryGetByAlias(string alias, out IAliased item)
        {
            return aliases.TryGetValue(alias, out item);
        }

        public bool TryGetByAlias<T>(string alias, out Aliased<T> item) where T : class
        {
            IAliased i;
            if (aliases.TryGetValue(alias, out i))
            {
                item = i as Aliased<T>;
                return item != null;
            }
            item = null;
            return false;
        }
    }
}
