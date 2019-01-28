using System;
using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Internals;
using Caasiope.Explorer.JSON.API.Responses;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;
using Helios.JSON;
using Ledger = Caasiope.Explorer.JSON.API.Internals.Ledger;
using TxDeclaration = Caasiope.Explorer.JSON.API.Internals.TxDeclaration;

namespace Caasiope.Explorer.JSON.API
{
    public class ResponseHelper
	{
		public static Response CreateGetBalanceResponse(Account account)
		{
		    return ReferenceEquals(account, null) ? new GetBalanceResponse() : new GetBalanceResponse { Address = account.Address.Encoded, Balance = FormatBalance(account.Balances)};
		}

		public static Response CreateGetAccountResponse(Account account)
		{
		    return ReferenceEquals(account, null) ? new GetAccountResponse() : new GetAccountResponse { Address = account.Address.Encoded, Balance = FormatBalance(account.Balances), Declaration = GetDeclaration(account.Declaration)};
		}

	    private static TxDeclaration GetDeclaration(Protocol.Types.TxAddressDeclaration declaration)
	    {
	        return ReferenceEquals(declaration, null) ? null : TransactionConverter.CreateDeclaration(declaration);
	    }

	    public static Response CreateGetTransactionResponse(Internals.Transaction transaction = null)
		{
		    return transaction == null ? new GetTransactionResponse() : new GetTransactionResponse {Transaction = transaction};
		}

	    public static Response CreateGetTransactionHistoryResponse(List<Internals.HistoricalTransaction> transactions = null, int? total = null)
		{
		    return transactions == null ? new GetTransactionHistoryResponse() : new GetTransactionHistoryResponse { Transactions = transactions, Total = total};
		}

	    public static Response CreateGetLedgerResponse(Ledger ledger = null)
		{
		    return ledger == null ? new GetLedgerResponse() : new GetLedgerResponse { Ledger = ledger };
		}

	    public static Response CreateGetLatestLedgersResponse(List<Ledger> ledgers = null)
		{
		    return ledgers == null ? new GetLatestLedgersResponse() : new GetLatestLedgersResponse { Ledgers = ledgers };
		}

	    public static Response CreateGetOrderBookResponse(List<Order> orderbook, string symbol)
	    {
	        return new GetOrderBookResponse() {Orders = orderbook, Symbol = symbol};
	    }

	    private static Dictionary<string, decimal> FormatBalance(IEnumerable<AccountBalance> balances)
		{
			var dictionary = new Dictionary<string, decimal>();

			foreach (var balance in balances)
			{
				dictionary.Add(Currency.ToSymbol(balance.Currency), Amount.ToWholeDecimal(balance.Amount));
			}

			return dictionary;
		}

	    public static Response CreateSendTransactionResponse(TransactionHash hash = null)
	    {
	        return new SendTransactionResponse {Hash = hash?.ToBase64()};
	    }

	    public static TransactionHash ReadSendTransactionResponse(SendTransactionResponse response)
	    {
	        return response.Hash == null ? null : new TransactionHash(Convert.FromBase64String(response.Hash));
	    }

	    public static GetSignedLedgerResponse CreateGetSignedLedgerResponse(SignedLedger signed = null)
	    {
	        return new GetSignedLedgerResponse
	        {
	            Ledger = signed == null ? null : ToBase64<ByteStream>(stream => { stream.Write(signed); })
	        };
	    }

	    public static bool TryReadSignedLedger(GetSignedLedgerResponse response, out SignedLedger signed)
	    {
	        try
	        {
	            using (var stream = new ByteStream(Convert.FromBase64String(response.Ledger)))
	            {
	                signed = stream.ReadSignedLedger();
	            }
	        }
	        catch
	        {
	            signed = null;
	            return false;
	        }
	        return true;
	    }

	    private static string ToBase64<T>(Action<T> write) where T : ByteStream, new()
	    {
	        byte[] bytes;
	        using (var stream = new T())
	        {
	            write(stream);
	            bytes = stream.GetBytes();
	        }
	        return Convert.ToBase64String(bytes);
	    }

	    public static Response CreateSubscribeResponse()
	    {
	        return new SubscribeResponse();
	    }
	}
}
