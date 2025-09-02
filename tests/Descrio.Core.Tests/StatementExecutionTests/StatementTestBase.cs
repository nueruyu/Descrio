using NUnit.Framework;

namespace Descrio.EditorTests.StatementExecutionTests
{
    public abstract class StatementTestBase
    {
        protected TestEnvironment Env { get; private set; }

        [SetUp]
        public void BaseSetUp()
        {
            Env = TestEnvironment.Create();
        }
    }
}
