using NUnit.Framework;
using Descrio.Parse.Expressions;
using System.Collections.Generic;
using System.Linq;
using static Descrio.Parse.Expressions.TokenType;
using System;

namespace Descrio.EditorTests
{
    [TestFixture]
    public class TokenizerTests
    {
        private List<Token> Scan(string source)
        {
            var tokenizer = new Tokenizer(source);
            // We remove the final EOF token for easier comparison in tests.
            return tokenizer.ScanTokens().Where(t => t.Type != EOF).ToList();
        }

        [Test]
        public void Scan_SingleTokens_ShouldRecognizeCorrectly()
        {
            var source = "+ - * / ( ) !";
            var tokens = Scan(source);

            var expectedTypes = new[] { PLUS, MINUS, STAR, SLASH, LEFT_PAREN, RIGHT_PAREN, BANG };
            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToList());
        }

        [Test]
        public void Scan_TwoCharacterTokens_ShouldRecognizeCorrectly()
        {
            var source = "!= == >= <=";
            var tokens = Scan(source);

            var expectedTypes = new[] { BANG_EQUAL, EQUAL_EQUAL, GREATER_EQUAL, LESS_EQUAL };
            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToList());
        }

        [Test]
        public void Scan_Literals_ShouldRecognizeCorrectly()
        {
            var source = "'hello world' 123 45.6 true false null my_var";
            var tokens = Scan(source);

            var expectedTypes = new[] { STRING, NUMBER, NUMBER, TRUE, FALSE, NULL, IDENTIFIER };
            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToList());

            Assert.AreEqual("hello world", tokens[0].Literal);
            Assert.AreEqual(123L, tokens[1].Literal);
            Assert.AreEqual(45.6d, tokens[2].Literal);
        }

        [Test]
        public void Scan_MixedExpression_ShouldProduceCorrectTokens()
        {
            var source = "player_hp - 20 > 0";
            var tokens = Scan(source);

            var expectedTypes = new[] { IDENTIFIER, MINUS, NUMBER, GREATER, NUMBER };
            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToList());
            Assert.AreEqual("player_hp", tokens[0].Lexeme);
        }

        [Test]
        public void Scan_NoWhitespace_ShouldTokenizeCorrectly()
        {
            var source = "1+2*(3-4)";
            var tokens = Scan(source);

            var expectedTypes = new[] { NUMBER, PLUS, NUMBER, STAR, LEFT_PAREN, NUMBER, MINUS, NUMBER, RIGHT_PAREN };
            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToList());
        }

        [Test]
        public void Scan_UnterminatedString_ShouldThrowFormatException()
        {
            var source = "'this is not closed";
            Assert.Throws<System.FormatException>(() => Scan(source));
        }

        [Test]
        public void Scan_UnexpectedCharacter_ShouldThrowFormatException()
        {
            var source = "a + &";
            Assert.Throws<System.FormatException>(() => Scan(source));
        }

        [Test]
        public void Scan_Keywords_ShouldBeCaseSensitive()
        {
            var source = "true TRUE false FALSE null NULL";
            var tokens = Scan(source);

            var expectedTypes = new[] { TRUE, IDENTIFIER, FALSE, IDENTIFIER, NULL, IDENTIFIER };
            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToList());
            Assert.AreEqual("TRUE", tokens[1].Lexeme);
        }

        [Test]
        public void Scan_DoubleEquals_ShouldProduceEqualityToken()
        {
            var source = "==";
            var tokens = Scan(source);

            Assert.AreEqual(1, tokens.Count, "Should produce a single token.");
            Assert.AreEqual(EQUAL_EQUAL, tokens[0].Type);
        }

        [Test]
        public void Scan_SingleEquals_ShouldThrowFormatException()
        {
            var source = "x = 5";
            var ex = Assert.Throws<FormatException>(() => Scan(source));
            StringAssert.Contains("Unexpected character '='", ex.Message);
        }

        [Test]
        public void Scan_StringWithEscapeSequences_ShouldBeUnescapedCorrectly()
        {
            // String contains: It's a "test" with a \ backslash.
            var source = @"'It\'s a \""test\"" with a \\ backslash.'";
            var tokens = Scan(source);

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(STRING, tokens[0].Type);
            Assert.AreEqual("It's a \"test\" with a \\ backslash.", tokens[0].Literal);
        }

        [Test]
        public void Scan_StringWithSpecialChars_ShouldBeUnescapedCorrectly()
        {
            var source = "'Line1\\nLine2\\tTabbed'";
            var tokens = Scan(source);

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(STRING, tokens[0].Type);
            Assert.AreEqual("Line1\nLine2\tTabbed", tokens[0].Literal);
        }
    }
}