using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    public static class RESettings
    {
        public static int RESSURECTION_TIME = 1000; 
        public static int SPREADTIME_MIN = 1500; 
        public static int SPREADTIME_MAX = 3000;
        public static Color SKINZOMBIE = new Color(0.37f, 0.48f, 0.35f, 1f);

        public static IntRange SPREADTIME => new IntRange(SPREADTIME_MIN, SPREADTIME_MAX);
    }
}
