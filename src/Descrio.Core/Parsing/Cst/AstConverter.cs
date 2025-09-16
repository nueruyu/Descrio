using Descrio.Abstractions;
using Descrio.Syntax;
using Descrio.Syntax.Expressions;
using Descrio.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.Parsing.Cst
{
    /// <summary>
    /// A static class responsible for converting a syntax-agnostic CST
    /// into a semantic Descrio Abstract Syntax Tree (AST).
    /// </summary>
    internal static class AstConverter
    {
        #region Main Conversion Methods

        public static Module FromCstToModule(CstNode cstNode)
        {
            var rootMapping = ExpectMapping(cstNode, "Script root");

            var statements = GetOptionalList(rootMapping, "statements", FromCstToStatement) ?? new List<IStatement>();
            var imports = GetOptionalList(rootMapping, "imports", node => ExpectScalar(node, "Import path").Value) ?? new List<string>();

            return new Module(statements.ToArray(), imports.ToArray(), cstNode.Location);
        }

        public static IStatement FromCstToStatement(CstNode cstNode)
        {
            if (cstNode.NodeType == NodeType.None)
            {
                throw new AstConversionException("A statement node must have a type hint (e.g., a YAML tag like !run).", cstNode.Location);
            }

            return cstNode.NodeType switch
            {
                // Variable Statements
                NodeType.Let => ToLetStatement(cstNode),
                NodeType.Var => ToVarStatement(cstNode),
                NodeType.Assign => ToAssignStatement(cstNode),

                // Control Flow
                NodeType.When => ToWhenStatement(cstNode),
                NodeType.For => ToForStatement(cstNode),
                NodeType.While => ToWhileStatement(cstNode),
                NodeType.Match => ToMatchStatement(cstNode),

                // Function and Scope
                NodeType.Function => ToFunctionStatement(cstNode),
                NodeType.Return => ToReturnStatement(cstNode),
                NodeType.Break => new BreakStatement(cstNode.Location),
                NodeType.Continue => new ContinueStatement(cstNode.Location),

                // Async and Error Handling
                NodeType.Run => ToRunStatement(cstNode),
                NodeType.Dispatch => ToDispatchStatement(cstNode),
                NodeType.Try => ToTryCatchStatement(cstNode),
                NodeType.Throw => ToThrowStatement(cstNode),
                NodeType.Assert => ToAssertStatement(cstNode),

                _ => throw new AstConversionException($"The node type '{cstNode.TypeHint ?? cstNode.NodeType.ToString()}' cannot be converted to a Statement.", cstNode.Location)
            };
        }

        public static IExpression FromCstToExpression(CstNode cstNode)
        {
            switch (cstNode.NodeType)
            {
                case NodeType.Expression:
                    var scalar = ExpectScalar(cstNode);
                    var innerExpression = new Expressions.ExpressionParser(scalar.Value).Parse();

                    return new EmbeddedExpression(innerExpression, cstNode.Location);

                // Expressions that can also be statements
                case NodeType.Run:
                    return ToRunStatement(cstNode);

                case NodeType.Dispatch:
                    return ToDispatchStatement(cstNode);

                case NodeType.Lambda:
                    return ToLambdaExpression(cstNode);

                case NodeType.None: // It's a literal or a structure without a type hint
                    return cstNode switch
                    {
                        ScalarCstNode s => new LiteralExpression(ValueConverter.Convert(s.Value), s.Location),
                        SequenceCstNode seq => new ListExpression(seq.Children.Select(FromCstToExpression).ToList(), seq.Location),
                        MappingCstNode map => new DictionaryExpression(
                            map.Children.ToDictionary(
                                kvp => FromCstToExpression(kvp.Key),
                                kvp => FromCstToExpression(kvp.Value)
                            ), map.Location),
                        _ => throw new AstConversionException("This node cannot be converted to an expression.", cstNode.Location)
                    };

                default:
                    throw new AstConversionException($"The node type '{cstNode.TypeHint ?? cstNode.NodeType.ToString()}' is not valid in an expression context.", cstNode.Location);
            }
        }

        #endregion Main Conversion Methods

        #region Statement & Expression Converters

        private static LetStatement ToLetStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var name = GetRequiredScalar(map, "name");
            var valueNode = GetRequiredNode(map, "value");
            return new LetStatement(name, FromCstToExpression(valueNode), cstNode.Location);
        }

        private static VarStatement ToVarStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var name = GetRequiredScalar(map, "name");
            var valueNode = GetRequiredNode(map, "value");
            return new VarStatement(name, FromCstToExpression(valueNode), cstNode.Location);
        }

        private static AssignStatement ToAssignStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var nameNode = GetRequiredNode(map, "name");
            var name = ExpectScalar(nameNode, "The 'name' for an assignment").Value;
            var valueNode = GetRequiredNode(map, "value");
            var target = new VariableExpression(name, nameNode.Location); // Assuming target is a simple variable
            return new AssignStatement(target, FromCstToExpression(valueNode), cstNode.Location);
        }

        private static WhenStatement ToWhenStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var caseNodes = GetRequiredList(map, "cases", node => node);
            var cases = caseNodes.Select(caseNode =>
            {
                var caseMap = ExpectMapping(caseNode, "A case block in !when");
                var condition = GetOptionalNode(caseMap, "condition");
                var thenBlock = GetRequiredList(caseMap, "then", FromCstToStatement);

                IExpression conditionExpr = condition != null ? FromCstToExpression(condition) : null;
                return new WhenCaseBlock(conditionExpr, thenBlock.ToArray(), caseNode.Location);
            }).ToArray();
            return new WhenStatement(cases, cstNode.Location);
        }

        private static ForStatement ToForStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var enumerableNode = GetRequiredNode(map, "in");
            var variableName = GetRequiredScalar(map, "as");
            var statements = GetRequiredList(map, "statements", FromCstToStatement);
            return new ForStatement(FromCstToExpression(enumerableNode), variableName, statements.ToArray(), cstNode.Location);
        }

        private static WhileStatement ToWhileStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var conditionNode = GetRequiredNode(map, "condition");
            var statements = GetRequiredList(map, "statements", FromCstToStatement);
            return new WhileStatement(FromCstToExpression(conditionNode), statements.ToArray(), cstNode.Location);
        }

        private static MatchStatement ToMatchStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var valueNode = GetRequiredNode(map, "value");
            var caseNodes = GetRequiredList(map, "cases", node => node);
            var defaultBlock = GetOptionalList(map, "default", FromCstToStatement);

            var cases = caseNodes.Select(caseNode =>
            {
                var caseMap = ExpectMapping(caseNode, "A case block in !match");
                var caseValueNode = GetRequiredNode(caseMap, "case");
                var thenBlock = GetRequiredList(caseMap, "then", FromCstToStatement);
                var caseValueExpr = FromCstToExpression(caseValueNode);

                if (caseValueExpr is not LiteralExpression literal)
                {
                    throw new AstConversionException("Case values in !match must be literals.", caseValueNode.Location);
                }

                return new MatchCaseBlock(literal.Value, thenBlock.ToArray(), caseNode.Location);
            }).ToList();

            return new MatchStatement(FromCstToExpression(valueNode), cases, defaultBlock?.ToArray(), cstNode.Location);
        }

        private static FunctionStatement ToFunctionStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var name = GetRequiredScalar(map, "name");
            var parameters = ToParameterDefinitions(map);
            var statements = GetRequiredList(map, "statements", FromCstToStatement);
            return new FunctionStatement(name, parameters, statements.ToArray(), cstNode.Location);
        }

        private static LambdaExpression ToLambdaExpression(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var parameters = ToParameterDefinitions(map);
            var statements = GetRequiredList(map, "statements", FromCstToStatement);
            return new LambdaExpression(parameters, statements.ToArray(), cstNode.Location);
        }

        private static ParameterDefinition[] ToParameterDefinitions(MappingCstNode parentMap)
        {
            var paramNodes = GetOptionalList(parentMap, "parameters", node => node);
            if (paramNodes == null)
                return Array.Empty<ParameterDefinition>();

            return paramNodes.Select(paramNode =>
            {
                var paramMap = ExpectMapping(paramNode, "A parameter definition");
                var name = GetRequiredScalar(paramMap, "name");
                var type = GetOptionalScalar(paramMap, "type");
                var defaultValueNode = GetOptionalNode(paramMap, "default");

                object defaultValueObject = null;
                if (defaultValueNode != null)
                {
                    if (FromCstToExpression(defaultValueNode) is LiteralExpression literal)
                    {
                        defaultValueObject = literal.Value;
                    }
                    else
                    {
                        throw new AstConversionException("Parameter default values must be literals.", defaultValueNode.Location);
                    }
                }
                return new ParameterDefinition(name, type, defaultValueObject, paramNode.Location);
            }).ToArray();
        }

        private static ReturnStatement ToReturnStatement(CstNode cstNode)
        {
            IExpression valueExpr = null;
            if (cstNode is ScalarCstNode scalar)
            {
                if (!string.IsNullOrEmpty(scalar.Value))
                    throw new AstConversionException(
                        "A !return statement cannot have a scalar value. " +
                        "Use a mapping with a 'value' key for returning a value.", scalar.Location);
            }
            else
            {
                var map = ExpectMapping(cstNode);
                var valueNode = GetOptionalNode(map, "value");
                valueExpr = valueNode != null ? FromCstToExpression(valueNode) : null;
            }

            return new ReturnStatement(valueExpr, cstNode.Location);
        }

        private static RunStatement ToRunStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var name = GetRequiredScalar(map, "name");
            var argsMap = GetOptionalMapping(map, "args");
            var argExpressions = new Dictionary<string, IExpression>();
            if (argsMap != null)
            {
                foreach (var (keyNode, valueNode) in argsMap.Children)
                {
                    argExpressions[keyNode.Value] = FromCstToExpression(valueNode);
                }
            }
            return new RunStatement(name, argExpressions, cstNode.Location);
        }

        private static DispatchStatement ToDispatchStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var name = GetRequiredScalar(map, "name");
            var argsMap = GetOptionalMapping(map, "args");
            var argExpressions = new Dictionary<string, IExpression>();
            if (argsMap != null)
            {
                foreach (var (keyNode, valueNode) in argsMap.Children)
                {
                    argExpressions[keyNode.Value] = FromCstToExpression(valueNode);
                }
            }
            return new DispatchStatement(name, argExpressions, cstNode.Location);
        }

        private static TryCatchStatement ToTryCatchStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var tryBlock = GetRequiredList(map, "statements", FromCstToStatement);
            var catchClauses = GetOptionalList(map, "catch", node =>
            {
                var clauseMap = ExpectMapping(node, "A catch clause");
                var errorName = GetOptionalScalar(clauseMap, "name");
                var varName = GetOptionalScalar(clauseMap, "as");
                var thenBlock = GetRequiredList(clauseMap, "then", FromCstToStatement);
                return new CatchClause(errorName, varName, thenBlock.ToArray(), node.Location);
            }) ?? new List<CatchClause>();
            var finallyBlock = GetOptionalList(map, "finally", FromCstToStatement);

            return new TryCatchStatement(tryBlock.ToArray(), catchClauses, finallyBlock?.ToArray(), cstNode.Location);
        }

        private static ThrowStatement ToThrowStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var name = GetRequiredScalar(map, "name");
            var argsMap = GetOptionalMapping(map, "args");
            var argExpressions = new Dictionary<string, IExpression>();
            if (argsMap != null)
            {
                foreach (var (keyNode, valueNode) in argsMap.Children)
                {
                    argExpressions[keyNode.Value] = FromCstToExpression(valueNode);
                }
            }
            return new ThrowStatement(name, argExpressions, cstNode.Location);
        }

        private static AssertStatement ToAssertStatement(CstNode cstNode)
        {
            var map = ExpectMapping(cstNode);
            var conditionNode = GetRequiredNode(map, "condition");
            var message = GetOptionalScalar(map, "message");
            return new AssertStatement(FromCstToExpression(conditionNode), message, cstNode.Location);
        }

        #endregion Statement & Expression Converters

        #region CST Traversal Helpers

        private static MappingCstNode ExpectMapping(CstNode node)
        {
            return ExpectMapping(node, node.TypeHint);
        }

        private static MappingCstNode ExpectMapping(CstNode node, string context)
        {
            if (node is not MappingCstNode mappingNode)
                throw new AstConversionException($"{context} must be a mapping (key-value pairs).", node.Location);
            return mappingNode;
        }

        private static ScalarCstNode ExpectScalar(CstNode node)
        {
            return ExpectScalar(node, node.TypeHint);
        }

        private static ScalarCstNode ExpectScalar(CstNode node, string context)
        {
            if (node is not ScalarCstNode scalarNode)
                throw new AstConversionException($"{context} must be a scalar value (like text or a number).", node.Location);
            return scalarNode;
        }

        private static CstNode GetRequiredNode(MappingCstNode parent, string key)
        {
            var kvp = parent.Children.FirstOrDefault(kvp => kvp.Key.Value == key);
            if (kvp.Key == null)
                throw new AstConversionException($"Missing required key '{key}' in mapping.", parent.Location);
            return kvp.Value;
        }

        private static CstNode GetOptionalNode(MappingCstNode parent, string key)
        {
            var kvp = parent.Children.FirstOrDefault(kvp => kvp.Key.Value == key);
            return kvp.Key != null ? kvp.Value : null;
        }

        private static string GetRequiredScalar(MappingCstNode parent, string key)
        {
            var node = GetRequiredNode(parent, key);
            return ExpectScalar(node, $"Value of '{key}'").Value;
        }

        private static string GetOptionalScalar(MappingCstNode parent, string key)
        {
            var node = GetOptionalNode(parent, key);
            return node != null ? ExpectScalar(node, $"Value of '{key}'").Value : null;
        }

        private static MappingCstNode GetOptionalMapping(MappingCstNode parent, string key)
        {
            var node = GetOptionalNode(parent, key);
            return node != null ? ExpectMapping(node, $"Value of '{key}'") : null;
        }

        private static List<T> GetRequiredList<T>(MappingCstNode parent, string key, Func<CstNode, T> converter)
        {
            var node = GetRequiredNode(parent, key);
            if (node is not SequenceCstNode sequenceNode)
                throw new AstConversionException($"Expected '{key}' to be a sequence (a list of items).", node.Location);
            return sequenceNode.Children.Select(converter).ToList();
        }

        private static List<T> GetOptionalList<T>(MappingCstNode parent, string key, Func<CstNode, T> converter)
        {
            var node = GetOptionalNode(parent, key);
            if (node == null)
                return null;

            if (node is not SequenceCstNode sequenceNode)
                throw new AstConversionException($"Expected '{key}' to be a sequence (a list of items).", node.Location);

            return sequenceNode.Children.Select(converter).ToList();
        }

        #endregion CST Traversal Helpers
    }
}