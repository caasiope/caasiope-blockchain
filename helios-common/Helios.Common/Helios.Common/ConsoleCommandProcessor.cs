using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Helios.Common
{
    public class ConsoleCommandProcessor
    {
        private readonly Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();
        private readonly List<Assembly> assemblies;

        public ConsoleCommandProcessor(List<Assembly> assemblies)
        {
            this.assemblies = assemblies;
        }

        public void Initialize()
        {
            var method = GetType().GetMethod("CreateCommand");
            var baseType = typeof(ConsoleCommand);

            foreach (var assembly in assemblies)
            {
                foreach (var type in GetTypesFromAssembly(assembly, baseType))
                {
                    var generic = method.MakeGenericMethod(type);
                    generic.Invoke(this, null);
                }
            }
        }

        public void CreateCommand<T>() where T : ConsoleCommand, new()
        {
            RegisterCommand(new T());
        }

        private void RegisterCommand(ConsoleCommand command)
        {
            commands.Add(command.Name, command);
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && (t.IsSubclassOf(baseType)));
        }

        public bool Run(string name, string[] args)
        {
            if (commands.TryGetValue(name, out var command))
            {
                command.RunCommand(args);
                return true;
            }
            return false;
        }

        public void RunCommand(string name, string[] args)
        {
            var argsFull = new List<string> { name };
            argsFull.AddRange(args);
            if (!Run(name, argsFull.ToArray()))
                Console.WriteLine("Unknown instruction !");
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("Please input instructions :");
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    continue;

                var args = line.Split(' ');

                var name = args[0];

                if (name == "exit")
                {
                    return;
                }

                if (!Run(name, args))
                {
                    Console.WriteLine("Unknown instruction !");
                }
            }
        }

        // TODO may be add a description to each command
        public IEnumerable<string> GetRegisteredCommands()
        {
            return commands.Keys;
        }
    }

    public abstract class ConsoleCommand
    {
        public readonly string Name;
        private readonly Dictionary<string, CommandArgument> arguments = new Dictionary<string, CommandArgument>();

        protected ConsoleCommand(string name)
        {
            Name = name;
        }

        protected ConsoleCommand()
        {
            var className = GetType().Name;
            var command = "Command";
            Debug.Assert(className.Substring(className.Length - command.Length) == command);
            Name = className.Substring(0, className.Length - command.Length).ToLower();
        }

        public void RunCommand(string[] args)
        {
            if (args.Length == arguments.Count + 1)
            {
                var i = 1;
                foreach (var argument in arguments.Values)
                {
                    argument.Value = args[i++];
                }

                try
                {
                    ExecuteCommand(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed : {0}", e.Message);
                }
            }
            else
            {
                ExplainCommand();
            }
        }

        protected abstract void ExecuteCommand(string[] args);

        private void ExplainCommand()
        {
            var builder = new StringBuilder($"Incorrect : {Name}");
            foreach (var argument in arguments.Keys)
            {
                builder.Append(" [");
                builder.Append(argument);
                builder.Append("]");
            }
            Console.WriteLine(builder.ToString());
        }

        protected CommandArgument RegisterArgument(CommandArgument argument)
        {
            arguments.Add(argument.Name, argument);
            return argument;
        }
    }

    public class CommandArgument
    {
        public readonly string Name;
        public string Value;

        public CommandArgument(string name)
        {
            Name = name;
        }
    }
}
