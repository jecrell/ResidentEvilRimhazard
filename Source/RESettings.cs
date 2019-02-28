using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public static class RESettings
    {
        public static int SPREADTIME_MIN = 1500; 
        public static int SPREADTIME_MAX = 3000;
        public static IntRange SPREADTIME => new IntRange(SPREADTIME_MIN, SPREADTIME_MAX);
    }
}
