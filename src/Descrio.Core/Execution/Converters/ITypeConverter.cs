namespace Descrio.Execution.Converters
{
    /// <summary>
    /// Provides an interface for converting an object from one type to another.
    /// This is primarily used to map script-side dictionaries to strongly-typed C# objects.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Converts the given object to the target type implemented by the converter.
        /// </summary>
        /// <param name="source">The source object to convert, typically an IDictionary&lt;object, object&gt;.</param>
        /// <returns>The converted, strongly-typed object.</returns>
        object Convert(object source);
    }
}