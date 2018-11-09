using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.JSON.API.Notifications;
using Caasiope.JSON.API.Requests;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;
using Helios.JSON;

namespace Caasiope.JSON.Helpers
{
    public static class RequestHelper
    {

        public static RequestMessage CreateRequest(Request request)
        {
            return new RequestMessage(request);
        }

        public static RequestMessage CreateSendSignedTransactionRequest(SignedTransaction transaction)
        {
            return CreateRequest(new SendTransactionRequest
            {
                Transaction = ByteStreamConverter.ToBase64<ByteStream>(stream => { stream.Write(transaction); })
            });
        }

        public static RequestMessage CreateAccountRequest(Account account)
        {
            return CreateRequest(new GetAccountRequest
            {
                Address = account.Address.Encoded
            });
        }

        public static RequestMessage CreateGetTransactionsRequest(List<TransactionHash> hashes)
        {
            return CreateRequest(new GetTransactionsRequest()
            {
                Hashes = hashes.Select(h => Convert.ToBase64String(h.Bytes)).ToList()
            });
		}

	    public static RequestMessage CreateGetLastSignedLedgerRequest()
	    {
		    return CreateRequest(new GetCurrentLedgerHeightRequest()
		    {
		    });
	    }

		public static bool TryReadSignedNewLedger(JSON.API.Notifications.SignedNewLedger request, out Protocol.Types.SignedNewLedger signed)
        {
            try
            {
                using (var stream = new ByteStream(Convert.FromBase64String(request.Ledger)))
                {
                    signed = stream.ReadSignedNewLedger();
                }
                return true;
            }
            catch
            {
                signed = null;
                return false;
            }
        }

        public static bool TryReadSignedTransaction(SendTransactionRequest request, out SignedTransaction signed)
        {
            try
            {
                using (var stream = new ByteStream(Convert.FromBase64String(request.Transaction)))
                {
                    signed = stream.ReadSignedTransaction();
                }
                return true;
            }
            catch
            {
                signed = null;
                return false;
            }
        }

        public static bool TryReadSignedTransaction(TransactionReceived request, out SignedTransaction signed)
        {
            try
            {
                using (var stream = new ByteStream(Convert.FromBase64String(request.Transaction)))
                {
                    signed = stream.ReadSignedTransaction();
                }
                return true;
            }
            catch
            {
                signed = null;
                return false;
            }
        }

        public static bool TryReadTransactionHashes(GetTransactionsRequest request, out List<TransactionHash> hashes)
        {
            hashes = new List<TransactionHash>();

            foreach (var hash in request.Hashes)
            {
                try
                {
                    using (var stream = new ByteStream(Convert.FromBase64String(hash)))
                    {
                        hashes.Add(stream.ReadTransactionHash());
                    }
                }
                catch
                {
                    hashes = null;
                    return false;
                }
            }
            return true;
        }

        public static RequestMessage CreateGetSignedLedgerRequest(long height)
        {
            return CreateRequest(new GetSignedLedgerRequest {Height = height});
        }
    }
}