using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class TxInputOutputFull
    {
        public readonly TxInputOutput TxInputOutput;
        public readonly TransactionHash TransactionHash;
        public readonly byte Index;

        public TxInputOutputFull(TxInputOutput txInputOutput, TransactionHash transactionHash, byte index)
        {
            TxInputOutput = txInputOutput;
            TransactionHash = transactionHash;
            Index = index;
        }

        public override bool Equals(object obj)
        {
            var input = obj as TxInputOutputFull;
            if (input == null)
                return false;
            return TransactionHash.Equals(input.TransactionHash) && TxInputOutput.Equals(input.TxInputOutput);
        }
    }
}