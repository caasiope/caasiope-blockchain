using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetValidatorsCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var validators = LiveService.ValidatorManager.GetValidators().ToList();
            Console.WriteLine($"Number of validators : {validators.Count}, Quorum : {LiveService.ValidatorManager.Quorum}");
            foreach (var validator in validators)
            {
                Console.WriteLine($"Public Key : {validator.ToBase64()}");
            }
        }
    }
}