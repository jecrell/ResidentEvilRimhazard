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
                    new Type[] { typeof(Pawn), typeof(IntVec3) }
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

            //Zombies are not considered colonists
            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn),
                    "get_IsColonist"
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(get_IsColonist_PostFix)
                    ),
                null);


            //Zombies are considered prisoners
            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn_GuestTracker),
                    "get_IsPrisoner"
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(get_IsPrisoner_PostFix)
                    ),
                null);



            //Everywhere zombies are in are dangerous
            //harmony.Patch(
            //    AccessTools.Method(
            //        typeof(Region),
            //        "DangerFor"
            //        ),
            //    null,
            //    new HarmonyMethod(
            //        typeof(HarmonyPatches),
            //        nameof(DangerForPostFix)
            //        ),
            //    null);

            //We can't wander where the zombies are
            harmony.Patch(
                AccessTools.Method(
                    typeof(JobGiver_WanderColony),
                    "GetWanderRoot"
                    ),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(GetWanderRoot_PostFix)
                    ),
                null);

            //Zombies shouldn't count towards social opinions
            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn_RelationsTracker),
                    "OpinionOf"
                    ),
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(OpinionOf_PreFix)
                    ),
                null);
        }

        public static bool OpinionOf_PreFix(Pawn_RelationsTracker __instance, Pawn other, ref int __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_RelationsTracker), "pawn").GetValue(__instance);
            if (other is Zombie || other is BOW || pawn is Zombie || pawn is BOW)
            {
                __result = 0;
                return false;
            }
            return true;
        }

        public static void GetWanderRoot_PostFix(JobGiver_WanderColony __instance, Pawn pawn, ref IntVec3 __result)
        {
            if (Find.Scenario.name == "Resident Evil")
            {
                if (pawn is Zombie) return;
                if (pawn.Spawned)
                {
                    var map = pawn.Map;
                    var ZombieDangerMap = map.GetComponent<ZombieDangerMap>();
                    if (!ZombieDangerMap.regionDangers.ContainsKey(__result.GetRegion(map)))
                    {
                        ZombieDangerMap.regionDangers.Add(__result.GetRegion(map), 1000);
                    }
                    if (ZombieDangerMap.regionDangers[__result.GetRegion(map)] > 0 || __result.Fogged(map))
                    {
                        __result = pawn.GetRegion().Cells.InRandomOrder().First(x => x.Standable(map));
                    }
                }
            }
        }

        ////// Verse.Region
        ////public static void DangerForPostFix(Region __instance, Pawn p, ref Danger __result)
        ////{
        ////    if (Current.ProgramState == ProgramState.Playing)
        ////    {
        ////        if (Find.Scenario.name == "Resident Evil")
        ////        {
        ////            if (p is Zombie) return;

        ////            Pawn pawn = (Pawn)AccessTools.Field(typeof(PawnUIOverlay), "pawn").GetValue(__instance);

        ////            if (p.RaceProps.Humanlike)
        ////            {
        ////                var zombieDangerMap = p.Map.GetComponent<ZombieDangerMap>();
        ////                if (zombieDangerMap == null) return;
        ////                if (!zombieDangerMap.regionDangers.ContainsKey(__instance))
        ////                {
        ////                    zombieDangerMap.regionDangers.Add(__instance, 1000);
        ////                }
        ////                if (zombieDangerMap.regionDangers[__instance] > 0)
        ////                    __result = Danger.Deadly;
        ////            }
        ////        }
        ////    }
        ////}

        public static void get_IsPrisoner_PostFix(Pawn_GuestTracker __instance, ref bool __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_GuestTracker), "pawn").GetValue(__instance);
            if (pawn is Zombie && pawn.Faction == Faction.OfPlayer)
            {
                __result = true;
            }
        }

        public static void get_IsColonist_PostFix(Pawn __instance, ref bool __result)
        {
            if (__instance is Zombie)
                __result = false;
        }

        public static void PawnCanOpenPostfix(Pawn p, ref bool __result)
        {
            var REScenarios = new List<string>
            { "Resident Evil",
            "Resident Evil - Umbrella Corp",
            "Resident Evil - Naked Brutality"};
            if (REScenarios.Any(x => x == Find.Scenario.name))
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
            var REScenarios = new List<string>
            { "Resident Evil",
            "Resident Evil - Naked Brutality"};
            if (REScenarios.Any(x => x == Find.Scenario.name))
            {
                CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell);
                MapGenerator.PlayerStartSpot = cell;
            }
            if (Find.Scenario.name == "Resident Evil - Umbrella Corp")
            {
                MapGenerator.PlayerStartSpot = map.Center;
            }
        }


        // RimWorld.ScenPart_PlayerPawnsArriveMethod
        public static void GenerateIntoMap(ScenPart_PlayerPawnsArriveMethod __instance, Map map)
        {
            if (Find.GameInitData != null)
            {
                var REScenarios = new List<string>
                {
                    "Resident Evil",
                    "Resident Evil - Umbrella Corp",
                    "Resident Evil - Naked Brutality"
                };
                if (REScenarios.Any(x => x == Find.Scenario.name))
                {
                    bool miniBaseCreated = false;
                    foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
                    {
                        if (startingAndOptionalPawn.Spawned)
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


                            //No zombies at spawnpoint
                            var zombiesNearby = map.mapPawns.AllPawnsSpawned.FindAll(p => (p is Zombie || p is BOW) && p.PositionHeld.DistanceToSquared(startingAndOptionalPawn.PositionHeld) < 10);
                            foreach (var zombie in zombiesNearby)
                            {
                                zombie.Destroy();
                            }

                            //All characters start off aggressive
                            startingAndOptionalPawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;

                            if (Find.Scenario.name == "Resident Evil")
                                ScenarioGen.CreateBeds(startingAndOptionalPawn, map, ThingDefOf.Bedroll, ThingDefOf.Cloth);
                            else if (Find.Scenario.name == "Resident Evil - Umbrella Corp")
                                ScenarioGen.CreateBeds(startingAndOptionalPawn, map, ThingDefOf.Bed, ThingDefOf.Steel);

                            //Unfog
                            try
                            {
                                AccessTools.Method(typeof(FogGrid), "FloodUnfogAdjacent").Invoke(map.fogGrid, new object[] { startingAndOptionalPawn.PositionHeld });
                            }
                            catch
                            {

                            }

                            //Create a minibase
                            if (miniBaseCreated)
                                continue;
                            miniBaseCreated = true;
                            
                            if (Find.Scenario.name == "Resident Evil - Umbrella Corp")
                                ScenarioGen.CreateBase(startingAndOptionalPawn, map);
                            else if (Find.Scenario.name == "Resident Evil")
                                ScenarioGen.CreateOutpost(startingAndOptionalPawn, map);

                        }
                        else
                        {
                            //Spawn dead bodies of other STARS members in the map.
                            CellFinder.TryFindBestPawnStandCell(startingAndOptionalPawn, out IntVec3 spot);
                            GenPlace.TryPlaceThing(startingAndOptionalPawn, spot, Find.AnyPlayerHomeMap, ThingPlaceMode.Near);
                            startingAndOptionalPawn.Kill(null);
                        }

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

