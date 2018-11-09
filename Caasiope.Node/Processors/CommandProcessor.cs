
namespace Caasiope.Node.Processors
{
    public abstract class CommandProcessor<T> : Helios.Common.Concepts.CQRS.CommandProcessor<T> where T : Helios.Common.Concepts.CQRS.ICommand
    {
        protected override void PrepareCommand(T command)
        {
            Injector.Inject(command);
        }
    }
}
