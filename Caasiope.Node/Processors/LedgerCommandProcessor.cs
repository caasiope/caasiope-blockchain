using Caasiope.Node.Services;

namespace Caasiope.Node.Processors
{

    public interface ILedgerCommand : Helios.Common.Concepts.CQRS.ICommand { }


    public abstract class LedgerCommand<T> : Helios.Common.Concepts.CQRS.Command<T>, ILedgerCommand
    {
        [Injected]
        public ILiveService LiveService;
        [Injected]
        public IDatabaseService DatabaseService;
        [Injected]
        public ILedgerService LedgerService;

        protected LedgerCommand(T data) : base(data) { }
    }

    internal class LedgerCommandProcessor : CommandProcessor<ILedgerCommand> { }
}
