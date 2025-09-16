using NUnit.Framework;
using Descrio.Data;

namespace Descrio.EditorTests
{
    public class ModulePathTests
    {
        [TestCase("/a/b", "c.yaml", "/a/b/c.yaml")]
        [TestCase("/a/b/", "c.yaml", "/a/b/c.yaml")]
        [TestCase("/", "c.yaml", "/c.yaml")]
        [TestCase("/a/b", "./c.yaml", "/a/b/c.yaml")]
        [TestCase("/a/b", "../c.yaml", "/a/c.yaml")]
        [TestCase("/a/b/c", "../../d.yaml", "/a/d.yaml")]
        [TestCase("/a/b", "/d/e.yaml", "/d/e.yaml")] // Absolute path should override base
        [TestCase("/", "../c.yaml", "/c.yaml")] // Cannot go up from root
        [TestCase("/a/b/../c", "./d.yaml", "/a/c/d.yaml")] // Complex path
        public void Resolve_ShouldReturnCorrectAbsolutePath(string basePath, string relativePath, string expected)
        {
            var baseModulePath = new ModulePath(basePath);
            var result = baseModulePath.Resolve(relativePath);
            Assert.AreEqual(expected, result.ToString());
        }

        [TestCase("/a/b/c.yaml", "/a/b")]
        [TestCase("/a/b/", "/a")]
        [TestCase("/a.yaml", "/")]
        [TestCase("/", "/")]
        [TestCase("", "/")] // Empty path should be treated as root directory
        public void GetDirectoryPath_ShouldReturnCorrectDirectory(string fullPath, string expected)
        {
            var modulePath = new ModulePath(fullPath);
            var result = modulePath.GetDirectoryPath();
            Assert.AreEqual(expected, result.ToString());
        }

        [Test]
        public void Constructor_ShouldNormalizeBackslashes()
        {
            var path = new ModulePath("a\\b\\c.yaml");
            Assert.AreEqual("a/b/c.yaml", path.ToString());
        }
    }
}