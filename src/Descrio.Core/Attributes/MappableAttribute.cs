using System;

namespace Descrio.Attributes
{
    /// <summary>
    /// Indicates that this type can be instantiated and populated from a script dictionary.
    /// <para>
    /// The Source Generator will create an <see cref="Descrio.Execution.Converters.ITypeConverter"/>
    /// for types marked with this attribute.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class MappableAttribute : Attribute
    {
    }
}