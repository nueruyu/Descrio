using Descrio.Data;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Execution
{
    internal static class BuiltInFunctions
    {
        public static async ValueTask<object> All(Arguments args, CancellationToken ct)
        {
            if (args.TryGetValue("tasks", out var tasksObj) && tasksObj is IEnumerable tasks)
            {
                var valueTasks = tasks.Cast<ValueTask<object>>().ToList();
                await Task.WhenAll(valueTasks.Select(vt => vt.AsTask()));
                return valueTasks.Select(vt => vt.Result).ToList();
            }
            return null;
        }

        public static async ValueTask<object> Any(Arguments args, CancellationToken ct)
        {
            if (args.TryGetValue("tasks", out var tasksObj) && tasksObj is IEnumerable tasks)
            {
                var valueTasks = tasks.Cast<ValueTask<object>>().ToList();
                var completedTask = await Task.WhenAny(valueTasks.Select(vt => vt.AsTask()));
                return await completedTask;
            }
            return null;
        }
    }
}