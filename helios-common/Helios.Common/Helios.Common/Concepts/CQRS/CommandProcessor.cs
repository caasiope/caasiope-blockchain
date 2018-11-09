using System.Collections.Concurrent;

namespace Helios.Common.Concepts.CQRS
{
	public abstract class CommandProcessor
	{
		public abstract class Command : ICommand
		{
			void ICommand.Process()
			{
				Process();
			}

			protected abstract void Process();
		}

		protected void ProcessCommand(ICommand command)
		{
			command.Process();
		}
	}

	public abstract class CommandProcessor<TCommand> : CommandProcessor where TCommand : ICommand
	{
		readonly ConcurrentQueue<TCommand> commands = new ConcurrentQueue<TCommand>();

		public void Add(TCommand command)
		{
			PrepareCommand(command);
			commands.Enqueue(command);
		}

		public bool TryProcessOne()
		{
			TCommand command;
			if (!commands.TryDequeue(out command)) return false;
			ProcessCommand(command);
			return true;
		}

	    public int Count
	    {
	        get { return commands.Count; }
	    }

	    protected abstract void PrepareCommand(TCommand command);
	}
}