using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    public class ZombieChance
    {
        public PawnKindDef zKind = null;
        public HediffDef zHediff = null;
        public float weight = 1;

        public ZombieChance(string pawnKind, string hediff, float newWeight)
        {
            zKind = PawnKindDef.Named(pawnKind);
            zHediff = HediffDef.Named(hediff);
            weight = newWeight;
        } 
    }

    public static class RESettings
    {
        public static int RESSURECTION_TIME = 1000;
        public static int SPREADTIME_MIN = 1000;
        public static int SPREADTIME_MAX = 4000;
        public static Color SKIN_ZOMBIE = new Color(0.37f, 0.48f, 0.35f, 1f);
        public static Color SKIN_CRIMSONHEAD = new Color(0.48f, 0.3f, 0.3f, 1f);
        public static Color SKIN_TYRANT = new Color(0.42f, 0.42f, 0.42f, 1f);
        public static float DEADZOMBIE_RESURRECTION_CHANCE = 0.4f;
        internal static int DEADZOMBIE_RESURRECTION_MINTIME = 1500;
        internal static int DEADZOMBIE_RESURRECTION_MAXTIME = 10000;
        public static int DOWNZOMBIE_TRANSFORM_INTERVAL_MIN = 2000;
        public static int DOWNZOMBIE_TRANSFORM_INTERVAL_MAX = 7000;
        internal static float MUTATION_CHANCE = 0.15f;
        public static bool DEBUG_MODE = true;

        public static IntRange SPREADTIME => new IntRange(SPREADTIME_MIN, SPREADTIME_MAX);


        public static List<ZombieChance> ResurrectedZombieTypeChanceTable = new List<ZombieChance>
            {
            new ZombieChance("RE_CrimsonHeadKind", "RE_TVirusCarrier_CrimsonHead", 100),
            new ZombieChance("RE_LickerKind", "RE_TVirusCarrier_Licker", 0),
            new ZombieChance("RE_TyrantKind", "RE_TVirusCarrier_Tyrant", 100)
            };
    
        public static void DM(string s)
        {
            if (DEBUG_MODE)
                Log.Message(s);
        }
    
    }


}
