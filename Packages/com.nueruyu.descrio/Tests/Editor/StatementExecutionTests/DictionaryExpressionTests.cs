using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class DictionaryExpressionTests : StatementTestBase
    {
        [Test]
        public async Task Dictionary_WithExpressionAsKey_ShouldEvaluateCorrectly()
        {
            // Arrange
            await Env.ExecuteAsync(Let("my_key", Literal("dynamic_key")));
            var dictExpr = Dictionary()
                .With(Variable("my_key"), Literal(123))
                .With(Literal("static_key"), Literal(456))
                .Build();

            // Act
            var result = await Env.ExecuteAsync(dictExpr);
            var dict = result.Value as Dictionary<object, object>;

            // Assert
            Assert.IsNotNull(dict);
            Assert.AreEqual(2, dict.Count);
            Assert.IsTrue(dict.ContainsKey("dynamic_key"));
            Assert.AreEqual(123L, dict["dynamic_key"]);
            Assert.IsTrue(dict.ContainsKey("static_key"));
            Assert.AreEqual(456L, dict["static_key"]);
        }

        [Test]
        public async Task Dictionary_WithComplexExpressionAsKey_ShouldEvaluateCorrectly()
        {
            // Arrange
            await Env.ExecuteAsync(Let("key_part", Literal("user_")));
            var dictExpr = Dictionary()
                .With(InterpolatedString(Variable("key_part"), Literal("id")), Literal(99))
                .Build();

            // Act
            var result = await Env.ExecuteAsync(dictExpr);
            var dict = result.Value as Dictionary<object, object>;

            // Assert
            Assert.IsNotNull(dict);
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.ContainsKey("user_id"));
            Assert.AreEqual(99L, dict["user_id"]);
        }
    }
}