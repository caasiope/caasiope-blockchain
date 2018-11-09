using System;
using System.Collections.Generic;
using Caasiope.JSON.Helpers;
using Caasiope.Node.Services;
using Caasiope.Node.Trackers;
using Caasiope.Node.Validators;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Node.Managers
{
    /// <summary>
    /// TODO this class caches ALL the transactions. WTF?
    /// </summary>
    public class TransactionManager
    {
        // maybe merge together

        [Injected] public IConnectionService ConnectionService;

        public TransactionValidator TransactionValidator;

        public Action<SignedTransaction> TransactionReceived;

        private readonly TransactionTracker transactionTracker = new TransactionTracker();

        public TransactionManager()
        {
            TransactionValidator = new TransactionValidator();
        }

        public void Initialize()
        {
            Injector.Inject(this);
            TransactionValidator.Initialize();
        }

        // TODO make this private
        public bool TryGetTransaction(TransactionHash hash, out SignedTransaction transaction)
        {
            return transactionTracker.TryGetTransaction(hash, out transaction);
        }

        // TODO make this private
        public bool IsExist(TransactionHash hash)
        {
            return transactionTracker.IsExist(hash);
        }

        // TODO move this
        public void SendTransactionReceivedNotification(SignedTransaction transaction)
        {
            // TODO Do not send back
            ConnectionService.BlockchainChannel.Broadcast(NotificationHelper.CreateTransactionReceivedNotification(transaction));
        }

        // add a transaction that was previously checked
        private void Add(SignedTransaction signed)
        {
            // check if it is a new transaction
            transactionTracker.Add(signed);
        }

        private bool Validate(SignedTransaction signed)
        {
            return TransactionValidator.Validate(signed);
        }

        public List<TransactionHash> GetAllHashes()
        {
            return transactionTracker.GetAllHashes();
        }

        // we received a transaction from the network
        public ResultCode ReceiveTransaction(SignedTransaction signed)
        {
            // 1 Check if the transaction already exist
            if (IsExist(signed.Hash))
            {
                return ResultCode.Success;
            }

            // 2 Validate transaction fields format
            if (!Validate(signed))
            {
                return ResultCode.TransactionValidationFailed;
            }

            // 3 Include transaction 
            Add(signed);
            
            // 4 notify new transaction
            TransactionReceived.Call(signed);

            return ResultCode.Success;
        }
    }
}