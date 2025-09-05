namespace Descrio.Yaml.Nodes
{
    internal class ContinueNode : IStatementNode
    {
        public IStatement ToStatement() => new ContinueStatement();
    }
}