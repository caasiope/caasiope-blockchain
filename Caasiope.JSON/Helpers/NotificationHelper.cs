using System;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;
using Helios.JSON;

namespace Caasiope.JSON.Helpers
{
    public static class NotificationHelper
    {
        public static NotificationMessage CreateNotification(Notification notification)
        {
            return new NotificationMessage(notification);
        }

        public static NotificationMessage CreateSignedNewLedgerNotification(SignedLedger ledger)
        {
            return CreateNotification(new API.Notifications.SignedNewLedger
            {
                Ledger = ByteStreamConverter.ToBase64<ByteStream>(stream => { stream.Write(new SignedNewLedger(ledger)); })
            });
        }

        public static NotificationMessage CreateTransactionReceivedNotification(SignedTransaction transaction)
        {
            return CreateNotification(new API.Notifications.TransactionReceived
            {
                Transaction = ByteStreamConverter.ToBase64<ByteStream>(stream => { stream.Write(transaction); })
            });
        }
    }
}