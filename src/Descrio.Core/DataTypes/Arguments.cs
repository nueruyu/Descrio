using System;
using System.Collections.Generic;

namespace Descrio
{
    public class Arguments : Dictionary<string, object>
    {
        public Arguments() : base(StringComparer.Ordinal)
        {
        }
    }
}