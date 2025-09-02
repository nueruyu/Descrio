using System;
using System.IO;
using Descrio.Yaml.Nodes;
using Descrio.Yaml.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Descrio.Parse;

namespace Descrio.Yaml
{
    public class YamlScriptParser : IScriptParser
    {
        private readonly IDeserializer _deserializer;

        public YamlScriptParser()
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTagMapping("!let", typeof(LetNode))
                .WithTagMapping("!var", typeof(VarNode))
                .WithTagMapping("!assign", typeof(AssignNode))
                .WithTagMapping("!run", typeof(RunNode))
                .WithTagMapping("!dispatch", typeof(DispatchNode))
                .WithTagMapping("!function", typeof(FunctionNode))
                .WithTagMapping("!lambda", typeof(LambdaNode))
                .WithTagMapping("!when", typeof(WhenNode))
                .WithTagMapping("!for", typeof(ForNode))
                .WithTagMapping("!while", typeof(WhileNode))
                .WithTagMapping("!return", typeof(ReturnNode))
                .WithTagMapping("!break", typeof(BreakNode))
                .WithTagMapping("!continue", typeof(ContinueNode))
                .WithTagMapping("!expr", typeof(ExpressionStringNode))
                .WithTagMapping("!match", typeof(MatchNode))
                .WithTagMapping("!try", typeof(TryCatchNode))
                .WithTagMapping("!throw", typeof(ThrowNode))
                .WithTagMapping("!assert", typeof(AssertNode))
                .WithNodeDeserializer(new ExpressionNodeDeserializer(), s => s.OnTop())
                .WithTypeConverter(new ExpressionStringNodeTypeConverter())
                .WithTypeConverter(new ContinueNodeTypeConverter())
                .WithTypeConverter(new BreakNodeTypeConverter())
                .WithTypeConverter(new ReturnNodeTypeConverter())
                .Build();
        }

        public Module Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new Module(Array.Empty<IStatement>(), Array.Empty<string>());
            }

            var scriptRootNode = _deserializer.Deserialize<ScriptRootNode>(new StringReader(text));

            return scriptRootNode?.ToModule() ?? new Module(Array.Empty<IStatement>(), Array.Empty<string>());
        }

        public IStatement ParseStatement(string text)
        {
            var statementNode = _deserializer.Deserialize<IStatementNode>(new StringReader(text));
            return statementNode?.ToStatement();
        }

        public IExpression ParseExpression(string text)
        {
            var expressionNode = _deserializer.Deserialize<IExpressionNode>(new StringReader(text));
            return expressionNode?.ToExpression();
        }
    }
}