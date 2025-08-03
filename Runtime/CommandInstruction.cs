using System;
using System.Threading.Tasks;

namespace Descrio
{
    public class CommandInstruction : IInstruction
    {
        public CommandInstruction(string commandName, IExpression[] argExpressions)
        {
            CommandName = commandName;
            ArgExpressions = argExpressions;
        }

        public string CommandName { get; }
        public IExpression[] ArgExpressions { get; }

        public async ValueTask ExecuteAsync(ExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!context.Callables.TryGet(CommandName, out var callable))
            {
                throw new InvalidOperationException($"Command or function '{CommandName}' not found.");
            }

            var args = new object[ArgExpressions.Length];
            for (int i = 0; i < ArgExpressions.Length; i++)
            {
                args[i] = await ArgExpressions[i].EvaluateAsync(context);
            }

            await callable.CallAsync(args, context.CancellationToken);
        }
    }
}