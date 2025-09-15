namespace Descrio.Data
{
    /// <summary>
    /// Represents a range in the source code (start and end positions).
    /// Both line and column are 1-indexed.
    /// </summary>
    public record SourceRange(int StartLine, int StartColumn, int EndLine, int EndColumn)
    {
        /// <summary>
        /// A singleton instance representing an unknown or unavailable location.
        /// </summary>
        public static readonly SourceRange Unknown = new(0, 0, 0, 0);

        public SourceRange WithOffset(int lineOffset, int columnOffset)
        {
            return new SourceRange(
                StartLine + lineOffset,
                StartColumn + columnOffset,
                EndLine + lineOffset,
                EndColumn + columnOffset
            );
        }
    }
}