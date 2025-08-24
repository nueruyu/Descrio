using System.Threading.Tasks;

namespace Descrio.Parse
{
    public interface IScriptParser
    {
        Module Parse(string text);
        IStatement ParseStatement(string text);
        IExpression ParseExpression(string text);
    }
}