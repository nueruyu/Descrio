using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;
using System.Collections.Generic;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class AssignStatementTests : StatementTestBase
    {
        [Test]
        public async Task Assign_ShouldAssignValueToVariable()
        {
            // Arrange
            await Env.ExecuteAsync(Var("x", Literal(10)));
            var statement = Assign(Variable("x"), Literal(20));

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            var result = await Env.ExecuteAsync(Variable("x"));
            Assert.AreEqual(20, result.Value);
        }

        [Test]
        public async Task Assign_ShouldAssignValueToDictionaryMember()
        {
            // Arrange
            var dict = new Dictionary<string, object>();
            await Env.ExecuteAsync(Var("dict", Literal(dict)));
            var statement = Assign(Variable("dict").Member("foo"), Literal("bar"));

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            Assert.AreEqual("bar", dict["foo"]);
        }

        public class MyClass
        {
            public string MyProperty { get; set; }
            public string MyField;
        }

        [Test]
        public async Task Assign_ShouldAssignValueToProperty()
        {
            // Arrange
            var myObject = new MyClass();
            await Env.ExecuteAsync(Var("obj", Literal(myObject)));
            var statement = Assign(Variable("obj").Member("MyProperty"), Literal("newValue"));

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            Assert.AreEqual("newValue", myObject.MyProperty);
        }

        [Test]
        public async Task Assign_ShouldAssignValueToField()
        {
            // Arrange
            var myObject = new MyClass();
            await Env.ExecuteAsync(Var("obj", Literal(myObject)));
            var statement = Assign(Variable("obj").Member("MyField"), Literal("newValue"));

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            Assert.AreEqual("newValue", myObject.MyField);
        }
    }
}