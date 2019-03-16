using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;

namespace RERimhazard
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.rerimhazard");

            //Zombies shamble
            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn_PathFollower),
                    "CostToMoveIntoCell",
                    new Type[] {typeof(Pawn), typeof(IntVec3)}
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(CostToMoveIntoCell_PostFix)
                    ),
                null);

            //Disables Zombie thoughts
            harmony.Patch(
                AccessTools.Method(
                    typeof(ThoughtUtility),
                    "CanGetThought"
                    ),
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(CanGetThought_PreFix)
                    ),
                null);
        }

        //Zombies don't receive thoughts
        public static bool CanGetThought_PreFix(Pawn pawn, ThoughtDef def, ref bool __result)
        {
            if (ZombieUtility.IsZombie(pawn))
            {
                __result = false;
                return false;
            }
            return true;
        }

        //Zombies will "shamble"
        public static void CostToMoveIntoCell_PostFix(Pawn pawn, IntVec3 c, ref int __result)
        {
            if (ZombieUtility.IsZombie(pawn))
            {
                var cHead = pawn?.kindDef?.defName == "RE_CrimsonHeadKind";
                var randPct = Rand.Range(0.4f * (cHead ? 0.2f : 1f), 3.2f * (cHead ? 0.5f : 1f));
                __result = Mathf.Clamp((int)(__result * randPct), 1, int.MaxValue);
            }
        }
    }
}
