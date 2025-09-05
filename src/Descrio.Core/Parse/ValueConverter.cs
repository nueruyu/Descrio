using System;
using System.Globalization;

namespace Descrio.Parse
{
    public static class ValueConverter
    {
        public static object Convert(string stringValue)
        {
            if (stringValue == null)
                return null;

            if (bool.TryParse(stringValue, out var boolResult))
            {
                return boolResult;
            }

            if (long.TryParse(
                stringValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var longResult))
            {
                return longResult;
            }

            if (double.TryParse(
                stringValue,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out var doubleResult))
            {
                return doubleResult;
            }

            return stringValue;
        }
    }
}