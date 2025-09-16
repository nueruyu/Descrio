using System;

namespace Descrio.Parsing.Utils
{
    /// <summary>
    /// Provides utility methods for calculating source code positions.
    /// </summary>
    public static class SourcePositionCalculator
    {
        /// <summary>
        /// Calculates the 1-based line and column of the character immediately preceding a given 1-based position.
        /// </summary>
        /// <param name="content">The full source text content.</param>
        /// <param name="index">The 0-based index of the position.</param>
        /// <param name="line">The 1-based line of the position.</param>
        /// <param name="column">The 1-based column of the position.</param>
        /// <returns>A tuple containing the (line, column) of the previous character.</returns>
        public static (int line, int column) GetPreviousCharacterLocation(string content, int index, int line, int column)
        {
            if (column > 1)
            {
                return (line, column - 1);
            }

            var prevLine = line - 1;
            if (prevLine < 1)
            {
                return (1, 1);
            }


            int indexOfLastChar = index - 1;

            if (indexOfLastChar > 0 && content[indexOfLastChar] == '\n')
            {
                indexOfLastChar--;
                if (indexOfLastChar >= 0 && content[indexOfLastChar] == '\r')
                {
                    indexOfLastChar--;
                }
            }

            if (indexOfLastChar < 0)
            {
                return (prevLine, 0);
            }

            var prevLineStartIndex = content.LastIndexOf('\n', indexOfLastChar);

            if (prevLineStartIndex == -1)
            {
                prevLineStartIndex = 0;
            }
            else
            {
                prevLineStartIndex += 1;
            }

            var finalColumn = (indexOfLastChar - prevLineStartIndex) + 1;

            return (prevLine, finalColumn);
        }
    }
}