using Caasiope.Protocol.Types;

namespace Caasiope.Node.Sagas
{
    // interface to interact with current and post state of the ledger
    public interface IUpdateStateSaga : IAccountList
    {
        // multisignature account need to be added to the state so we can validate transactions
        bool TryAddAccount(MultiSignature account);
        bool TryAddAccount(HashLock account);
        bool TryAddAccount(TimeLock account);
        void SetBalance(Account account, Currency currency, Amount amount);
    }

    public interface IAccountList
    {
        bool TryGetAccount(string encoded, out Account account);
        void AddAccount(Account account);
    }
}