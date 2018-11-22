using Caasiope.Protocol.Extensions;
using Caasiope.Protocol.MerkleTrees;

namespace Caasiope.Protocol.Types
{
    public interface ILedgerState
    {
        bool TryGetAccount(Address address, out Account account);
    }

    public abstract class LedgerState : ILedgerState
    {
        protected readonly Trie<Account> Tree;

        protected LedgerState(Trie<Account> tree)
        {
            Tree = tree;
        }

        protected static Trie<Account> GetTree(LedgerState state)
        {
            return state.Tree;
        }

        public abstract bool TryGetAccount(Address address, out Account account);
    }

    public static class LedgerStateExtensions
    {

        public static bool TryGetDeclaration<T>(this ILedgerState state, Address address, out T declaration) where T : TxAddressDeclaration
        {
            // if the account does exists in the state
            if (!state.TryGetAccount(address, out var account))
            {
                declaration = null;
                return false;
            }

            // check the account has been declared
            declaration = account.GetDeclaration<T>();
            return declaration != null;
        }
    }
}
