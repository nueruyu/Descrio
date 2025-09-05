namespace Descrio.Yaml.Nodes
{
    internal class BreakNode : IStatementNode
    {
        public IStatement ToStatement() => new BreakStatement();
    }
}