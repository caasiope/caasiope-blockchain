using System;
using Caasiope.Log;
using Caasiope.Node.Services;
using Helios.Common.Logs;

namespace Caasiope.Node.Processors
{
    public interface ILiveCommand : Helios.Common.Concepts.CQRS.ICommand
    {
        ResultCode ResultCode { get; }
        ILogger Logger { get; set; }
    }


    public abstract class LiveCommand<T> : Helios.Common.Concepts.CQRS.Command<T>, ILiveCommand
    {
        public ResultCode ResultCode { get; private set; }
        public ILogger Logger { get; set; }

        protected LiveCommand(T data) : base(data)
        {
        }

        protected sealed override void DoWork(T input)
        {
            try
            {
                ResultCode = GetResult(input);
            }
            catch (Exception)
            {
                ResultCode = ResultCode.Failed;
                throw;
            }
        }

        protected abstract ResultCode GetResult(T input);

        [Injected]
        public ILiveService LiveService;
        [Injected]
        public IDatabaseService DatabaseService;
        [Injected]
        public ILedgerService LedgerService;
    }

    internal class LiveCommandProcessor : CommandProcessor<ILiveCommand>
    {
        private readonly ILogger logger;

        public LiveCommandProcessor()
        {
            logger = new LoggerAdapter(GetType().Name);
        }

        protected override void PrepareCommand(ILiveCommand command)
        {
            base.PrepareCommand(command);
            command.Logger = logger;
        }
    }
}
