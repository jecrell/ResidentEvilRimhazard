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
using Verse.Sound;

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

            //Play "Resident Evil" sound when starting
            //  the scenario.
            harmony.Patch(
                AccessTools.Method(
                    typeof(Scenario),
                    "PostGameStart"
                    ),
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(PostGameStart_Prefix)
                    ),
                null);
            //Use custom names if we're doing the Resident Evil scenario
            harmony.Patch(
                AccessTools.Method(
                    typeof(PawnBioAndNameGenerator),
                    "GeneratePawnName"
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(GeneratePawnName)
                    ),
                null);

        }

        // RimWorld.PawnBioAndNameGenerator
        public static void GeneratePawnName(Pawn pawn, NameStyle style, string forcedLastName, ref Name __result)
        {
            if (pawn != null && pawn.Faction != null && pawn.Faction.def.defName == "RE_Player")
            {
                var ruleMaker = pawn.gender == 
                    Gender.Female ? 
                    DefDatabase<RulePackDef>.GetNamed("RE_STARSNamerFemale") 
                    :
                    DefDatabase<RulePackDef>.GetNamed("RE_STARSNamerMale");

                string rawName = NameGenerator.GenerateName(ruleMaker, delegate (string x)
                {
                    NameTriple nameTriple4 = NameTriple.FromString(x);
                    nameTriple4.ResolveMissingPieces(forcedLastName);
                    return !nameTriple4.UsedThisGame;
                });
                NameTriple nameTriple = NameTriple.FromString(rawName);
                nameTriple.CapitalizeNick();
                nameTriple.ResolveMissingPieces(forcedLastName);
                __result = nameTriple;
            }
        }

            // RimWorld.Scenario
            public static void PostGameStart_Prefix(Scenario __instance)
        {
            if (__instance == null || __instance.name == String.Empty) return;
            Log.Message("Scenario name: " + __instance.name);
            if (__instance.name == "Resident Evil")
                DefDatabase<SoundDef>.GetNamed("RE_Theme").PlayOneShotOnCamera();
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
