using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecer.ConsoleSplit
{
    internal struct TextSegment
    {
        internal string Text
        {
            get;
            set;
        }

        internal ConsoleColor ForegroundColor
        {
            get;
            set;
        }

        internal ConsoleColor BackgroundColor
        {
            get;
            set;
        }

        internal bool PrintOnce
        {
            get;
            set;
        }
    }
}
