using System.Linq;
using Caasiope.Explorer.JSON.API.Requests;
using Caasiope.Protocol.Types;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API
{
    public class RequestHelper
    {
        public static bool TryReadSignedTransaction(SendTransactionRequest message, out SignedTransaction signed)
        {
            try
            {
                signed = TransactionConverter.GetSignedTransaction(message.Transaction, message.Signatures);
                return signed != null;
            }
            catch
            {
                signed = null;
                return false;
            }
        }

        public static RequestMessage CreateSendTransactionRequest(SignedTransaction signed)
        {
            return CreateRequest(new SendTransactionRequest
            {
                Transaction = TransactionConverter.GetTransaction(signed.Transaction),
                Signatures = TransactionConverter.GetSignatures(signed.Signatures).ToList()
            });
        }

        public static RequestMessage CreateGetSignedLedgerRequest(long? height)
        {
            return CreateRequest(new GetSignedLedgerRequest
            {
                Height = height
            });
        }

        public static RequestMessage CreateRequest(Request request)
        {
            return new RequestMessage(request);
        }
    }
}