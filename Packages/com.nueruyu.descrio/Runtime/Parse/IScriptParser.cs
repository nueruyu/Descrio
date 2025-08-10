using System.Threading.Tasks;

namespace Descrio.Parse
{
    public interface IScriptParser
    {
        Module Parse(string scriptText);
    }
}