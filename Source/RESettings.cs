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
        public static float DEADZOMBIE_RESURRECTION_CHANCE = 1.0f;
        internal static int DEADZOMBIE_RESURRECTION_MINTIME = 1500;
        internal static int DEADZOMBIE_RESURRECTION_MAXTIME = 2000;
        internal static float MUTATION_CHANCE = 0.15f;

        public static IntRange SPREADTIME => new IntRange(SPREADTIME_MIN, SPREADTIME_MAX);
    }
}
