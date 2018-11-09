using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;

namespace Caasiope.Wallet.Listeners
{
    // this class will track every transaction that was submitted to the network
    // it will say which transaction was included or not
    public class TransactionSubmissionListener
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly Dictionary<TransactionHash, TransactionSubmission> transactions = new Dictionary<TransactionHash, TransactionSubmission>();
        private int nbSuccess = 0;
        private int nbFailure = 0;

        public Action<SignedTransaction, TimeSpan> OnSuccess;

        public void OnSubmitted(SignedTransaction transaction)
        {
            using (locker.CreateLock())
            {
                transactions.Add(transaction.Hash, new TransactionSubmission(transaction));
            }
        }

        public void OnLedgerUpdated(Ledger ledger)
        {
            using (locker.CreateLock())
            {
                List<TransactionSubmission> removeds = new List<TransactionSubmission>();
                var timestamp = ledger.LedgerLight.Timestamp;

                foreach (var signed in ledger.Block.Transactions)
                {
                    // included
                    if (transactions.TryGetValue(signed.Hash, out var submission))
                    {
                        Success(submission, removeds);
                    }
                }

                foreach (var submission in transactions.Values)
                {
                    if (submission.Signed.Transaction.Expire <= timestamp)
                    {
                        Failure(submission, removeds);
                    }
                }

                foreach (var transaction in removeds)
                {
                    transactions.Remove(transaction.Signed.Hash);
                }
            }

        }

        private void Success(TransactionSubmission submission, List<TransactionSubmission> removeds)
        {
            removeds.Add(submission);
            nbSuccess++;
            var elapsed = DateTime.Now - submission.SubmissionTime;
            OnSuccess.Call(submission.Signed, elapsed);
        }

        private void Failure(TransactionSubmission submission, List<TransactionSubmission> removeds)
        {
            removeds.Add(submission);
            nbFailure++;
        }

        public void GetStatistics(out int success, out int failure, out int pending)
        {
            using (locker.CreateLock())
            {
                success = nbSuccess;
                failure = nbFailure;
                pending = transactions.Count;
            }
        }
    }

    internal class TransactionSubmission
    {
        public readonly SignedTransaction Signed;
        public readonly DateTime SubmissionTime;

        public TransactionSubmission(SignedTransaction signed)
        {
            SubmissionTime = DateTime.Now;
            Signed = signed;
        }
    }
}
