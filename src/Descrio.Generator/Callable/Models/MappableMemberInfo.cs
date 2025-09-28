namespace Descrio.Generator.Callable.Models
{
    /// <summary>
    /// Holds information about a field or property that can be mapped
    /// from a dictionary value in a script.
    /// </summary>
    internal class MappableMemberInfo
    {
        /// <summary>
        /// The name of the member (e.g., "Name", "Power").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The fully qualified name of the member's type (e.g., "global::System.String").
        /// </summary>
        public string TypeName { get; set; }
    }
}