using Descrio.Data;
using Descrio.Parsing.Cst;
using Descrio.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Descrio.Parsing.Yaml
{
    /// <summary>
    /// Builds a common Concrete Syntax Tree (CST) from a YAML source.
    /// </summary>
    public class YamlCstBuilder
    {
        private static readonly Dictionary<string, NodeType> TagToNodeTypeMap = new()
        {
            { "!let", NodeType.Let },
            { "!var", NodeType.Var },
            { "!assign", NodeType.Assign },
            { "!when", NodeType.When },
            { "!for", NodeType.For },
            { "!while", NodeType.While },
            { "!match", NodeType.Match },
            { "!function", NodeType.Function },
            { "!return", NodeType.Return },
            { "!break", NodeType.Break },
            { "!continue", NodeType.Continue },
            { "!try", NodeType.Try },
            { "!throw", NodeType.Throw },
            { "!assert", NodeType.Assert },
            { "!run", NodeType.Run },
            { "!dispatch", NodeType.Dispatch },
            { "!expr", NodeType.Expression },
            { "!lambda", NodeType.Lambda },
        };

        private IParser _parser;

        public CstRoot Build(string yamlContent)
        {
            var errors = new List<SyntaxError>();
            CstNode? root = null;

            if (string.IsNullOrWhiteSpace(yamlContent))
            {
                return new CstRoot(null, errors);
            }

            try
            {
                _parser = new Parser(new StringReader(yamlContent));

                _parser.Consume<StreamStart>();
                _parser.TryConsume<DocumentStart>(out _);

                switch (_parser.Current)
                {
                    case DocumentEnd:
                    case StreamEnd:
                        break;

                    default:
                        root = ParseNode();
                        break;
                }

                _parser.TryConsume<DocumentEnd>(out _);
                _parser.Consume<StreamEnd>();

                if (_parser.Current != null)
                {
                    var extraToken = _parser.Current;
                    errors.Add(new SyntaxError(
                        $"Unexpected content found after the main YAML document. Found token of type '{extraToken.GetType().Name}'.",
                        ToSourceRange(extraToken.Start, extraToken.End))
                    );
                }
            }
            catch (YamlException ex)
            {
                errors.Add(new SyntaxError(ex.Message, ToSourceRange(ex.Start, ex.End)));
            }

            return new CstRoot(root, errors);
        }

        private CstNode ParseNode()
        {
            var currentEvent = _parser.Current;
            return currentEvent switch
            {
                Scalar scalar => ParseScalar(scalar),
                SequenceStart seqStart => ParseSequence(seqStart),
                MappingStart mapStart => ParseMapping(mapStart),
                _ => throw new YamlException(currentEvent.Start, currentEvent.End, $"Unexpected parsing event: {currentEvent.GetType().Name}")
            };
        }

        private ScalarCstNode ParseScalar(Scalar scalar)
        {
            var rawTag = scalar.Tag.IsEmpty ? null : scalar.Tag.Value;
            var typeHint = GetNodeTypeFromTag(rawTag);
            _parser.MoveNext();

            var location = ToSourceRange(
                scalar.Start.Line,
                scalar.Start.Column,
                scalar.End.Line,
                scalar.End.Column - 1);

            return new ScalarCstNode(scalar.Value, typeHint, rawTag, location);
        }

        private SequenceCstNode ParseSequence(SequenceStart startEvent)
        {
            var rawTag = startEvent.Tag.IsEmpty ? null : startEvent.Tag.Value;
            var typeHint = GetNodeTypeFromTag(rawTag);
            var children = new List<CstNode>();
            _parser.MoveNext(); // Consume SequenceStart

            while (_parser.Current is not SequenceEnd)
            {
                children.Add(ParseNode());
            }

            var endEvent = _parser.Consume<SequenceEnd>();

            SourceRange location;
            if (children.Count > 0 && startEvent.Style == SequenceStyle.Block)
            {
                location = ToSourceRange(startEvent.Start, children.Last().Location);
            }
            else
            {
                location = ToSourceRange(startEvent.Start, endEvent.End);
            }

            return new SequenceCstNode(children, typeHint, rawTag, location);
        }

        private MappingCstNode ParseMapping(MappingStart startEvent)
        {
            var rawTag = startEvent.Tag.IsEmpty ? null : startEvent.Tag.Value;
            var typeHint = GetNodeTypeFromTag(rawTag);
            var children = new Dictionary<ScalarCstNode, CstNode>();
            _parser.MoveNext(); // Consume MappingStart

            while (_parser.Current is not MappingEnd)
            {
                if (ParseNode() is not ScalarCstNode keyNode)
                {
                    var keyEvent = _parser.Current;
                    throw new YamlException(keyEvent.Start, keyEvent.End, "Mapping keys must be scalar values.");
                }
                var valueNode = ParseNode();
                children[keyNode] = valueNode;
            }

            var endEvent = _parser.Consume<MappingEnd>();

            SourceRange location;
            if (children.Count > 0 && startEvent.Style == MappingStyle.Block)
            {
                location = ToSourceRange(startEvent.Start, children.Values.Last().Location);
            }
            else
            {
                location = ToSourceRange(startEvent.Start, endEvent.End);
            }

            return new MappingCstNode(children, typeHint, rawTag, location);
        }

        private NodeType GetNodeTypeFromTag(string? rawTag)
        {
            if (rawTag != null && TagToNodeTypeMap.TryGetValue(rawTag, out var nodeType))
            {
                return nodeType;
            }
            return NodeType.None;
        }

        private static SourceRange ToSourceRange(long startLine, long startColumn, long endLine, long endColumn)
        {
            return new SourceRange((int)startLine, (int)startColumn, (int)endLine, (int)endColumn);
        }

        private static SourceRange ToSourceRange(Mark start, Mark end)
        {
            return ToSourceRange(start.Line, start.Column, end.Line, end.Column);
        }

        private static SourceRange ToSourceRange(Mark start, SourceRange endLocation)
        {
            return ToSourceRange(
                start.Line,
                start.Column,
                endLocation.EndLine,
                endLocation.EndColumn);
        }
    }
}