using System;

namespace Descrio
{
    /// <summary>
    /// Marks a method to be exposed as a callable function from within a Descrio script.
    /// <para>
    /// Applying this attribute triggers the <b>Descrio.Generator</b> Source Generator at compile time.
    /// The generator creates a strongly-typed <c>AddCallables</c> extension method for the <see cref="ScriptRunner"/> class.
    /// This allows for high-performance, AOT-safe registration of all callable methods within the containing class.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <b>Example of usage:</b>
    /// <code>
    /// public class MyGameFunctions
    /// {
    ///     [Callable("ShowMessage")]
    ///     public void DisplayMessage(string text) { /* ... */ }
    ///
    ///     [Callable]
    ///     public int Add(int a, int b) => a + b;
    /// }
    ///
    /// // To register these methods:
    /// var myFunctions = new MyGameFunctions();
    /// var runner = new ScriptRunner(parser, provider);
    ///
    /// // The generated extension method for MyGameFunctions is called here.
    /// runner.AddCallables(myFunctions);
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CallableAttribute : Attribute
    {
        /// <summary>
        /// The name of the function as it will be called from the script.
        /// If null or empty, the method's name will be used as the callable name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallableAttribute"/> class.
        /// </summary>
        /// <param name="name">The optional name of the function for script-side calls. If not provided, the method's name is used.</param>
        public CallableAttribute(string name = null)
        {
            Name = name;
        }
    }
}