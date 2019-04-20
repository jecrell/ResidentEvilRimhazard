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

            //Place the starting characters at the end of the map.
            harmony.Patch(
                AccessTools.Method(
                    typeof(GenStep_FindPlayerStartSpot),
                    "Generate"
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(GeneratePostFix)
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

            
            harmony.Patch(
                AccessTools.Method(
                    typeof(ScenPart_PlayerPawnsArriveMethod),
                    "GenerateIntoMap"
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(GenerateIntoMap)
                    ),
                null);



            //Disable zombie names on the map
            harmony.Patch(
                AccessTools.Method(
                    typeof(PawnUIOverlay),
                    "DrawPawnGUIOverlay"
                    ),
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(DrawPawnGUIOverlayPrefix)
                    ),
                null);

            //Zombies don't have relationship memories (for now)
            harmony.Patch(
                AccessTools.Method(
                    typeof(RelationsUtility),
                    "HasAnySocialMemoryWith"
                    ),
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(HasAnySocialMemoryWithPrefix)
                    ),
                null);

            //Regular doors can be opened in the Resident Evil scenario
            // ... unless they are locked?
            harmony.Patch(
                AccessTools.Method(
                    typeof(Building_Door),
                    "PawnCanOpen"
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(PawnCanOpenPostfix)
                    ),
                null);
        }

        public static void PawnCanOpenPostfix(Pawn p, ref bool __result)
        {
            if (Find.Scenario.name == "Resident Evil")
            {
                if (p.Faction == Faction.OfPlayer && p.MapHeld is Map m && m.IsPlayerHome)
                {
                    __result = true;
                }
            }

        }
        // RimWorld.RelationsUtility
        private static bool HasAnySocialMemoryWithPrefix(Pawn p, Pawn otherPawn, ref bool __result)
        {
            if (p is Zombie || otherPawn is Zombie)
            {
                __result = false;
                return false;
            }
            return true;
        }

            // Verse.PawnUIOverlay
            public static bool DrawPawnGUIOverlayPrefix(PawnUIOverlay __instance)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(PawnUIOverlay), "pawn").GetValue(__instance);
            if (pawn is Zombie)
            {
                return false;
            }
            return true;
        }

            //GenStep_FindPlayerStartSpot
            public static void GeneratePostFix(Map map, GenStepParams parms)
    {
            if (Find.Scenario.name == "Resident Evil")
            {
                CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell);
                MapGenerator.PlayerStartSpot = cell;
            }
    }


    // RimWorld.ScenPart_PlayerPawnsArriveMethod
    public static void GenerateIntoMap(ScenPart_PlayerPawnsArriveMethod __instance, Map map)
        {
            if (Find.GameInitData != null)
            {
                foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
                {
                    //Give K9 backstory characters a Doberman.
                    if (startingAndOptionalPawn.Spawned &&
                        startingAndOptionalPawn.story != null &&
                        startingAndOptionalPawn.story.childhood != null &&
                        startingAndOptionalPawn.story.childhood.title == "Police Cadet (K9)")
                    {
                        var pawn = PawnGenerator.GeneratePawn(PawnKindDef.Named("RE_DobermanPinscherKind"), startingAndOptionalPawn.Faction);

                        GenPlace.TryPlaceThing(pawn, startingAndOptionalPawn.PositionHeld, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near);

                        pawn.training.Train(TrainableDefOf.Obedience, startingAndOptionalPawn, true);
                        pawn.playerSettings.Master = startingAndOptionalPawn;
                    }
                    //Spawn dead bodies of other STARS members in the map.
                    if (!startingAndOptionalPawn.Spawned)
                    {
                        CellFinder.TryFindBestPawnStandCell(startingAndOptionalPawn, out IntVec3 spot);
                        GenPlace.TryPlaceThing(startingAndOptionalPawn, spot, Find.AnyPlayerHomeMap, ThingPlaceMode.Near );
                        startingAndOptionalPawn.Kill(null);
                    }
                }
            }
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
            //Log.Message("Scenario name: " + __instance.name);
            //if (__instance.name == "Resident Evil")
            //    DefDatabase<SoundDef>.GetNamed("RE_Theme").PlayOneShotOnCamera();
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
