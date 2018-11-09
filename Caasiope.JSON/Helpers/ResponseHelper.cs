using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.JSON.API.Responses;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;
using Helios.JSON;

namespace Caasiope.JSON.Helpers
{
    public static class ResponseHelper
    {
        public static GetAccountResponse CreateGetAccountResponse(Account account)
        {
            return ReferenceEquals(account, null) ? new GetAccountResponse() : new GetAccountResponse { Account = ByteStreamConverter.ToBase64<ByteStream>(stream => { stream.Write(account); })};
        }

        public static GetTransactionsResponse CreateGetTransactionResponse(List<SignedTransaction> transactions = null)
        {
            var list = transactions == null ? null : transactions.Select(signed => ByteStreamConverter.ToBase64<ByteStream>(stream => { stream.Write(signed); })).ToList();

            return new GetTransactionsResponse { Transactions = list };
        }

        public static GetSignedLedgerResponse CreateGetSignedLedgerResponseFromZip(byte[] signed)
        {
            return new GetSignedLedgerResponse
            {
                Ledger = signed == null ? null : Convert.ToBase64String(signed)
            };
        }

        public static GetCurrentLedgerHeightResponse CreateGetCurrentLedgerHeightResponse(long height)
        {
            return new GetCurrentLedgerHeightResponse
            {
                Height = height
            };
        }

        public static ResponseMessage CreateResponseMessage(Response response, string crid, byte resultCode)
        {
            return new ResponseMessage(response, crid, resultCode);
        }

        public static bool TryReadSignedTransactions(GetTransactionsResponse response, out List<SignedTransaction> transactions)
        {
            transactions = new List<SignedTransaction>();
            foreach (var signedTransaction in response.Transactions)
            {
                try
                {
                    using (var stream = new ByteStream(Convert.FromBase64String(signedTransaction)))
                    {
                        transactions.Add(stream.ReadSignedTransaction());
                    }
                }
                catch
                {
                    transactions = null;
                    return false;
                }
            }
            return true;
        }

        public static bool TryReadSignedLedger(GetSignedLedgerResponse response, out SignedLedger signed)
        {
            try
            {
                signed = LedgerCompressionEngine.ReadZippedLedger(Convert.FromBase64String(response.Ledger));
            }
            catch
            {
                signed = null;
                return false;
            }
            return true;
        }

        public static bool TryReadAccount(GetAccountResponse response, out Account account)
        {
            try
            {
                using (var stream = new ByteStream(Convert.FromBase64String(response.Account)))
                {
                    account = stream.ReadAccount();
                }
            }
            catch
            {
	            account = null;
                return false;
            }
            return true;
        }
    }
}