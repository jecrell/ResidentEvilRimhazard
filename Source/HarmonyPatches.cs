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

            //Umbrella scenario demands umbrella characters
            harmony.Patch(
                AccessTools.Method(
                    typeof(StartingPawnUtility),
                    "NewGeneratedStartingPawn"
                    ),
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(NewGeneratedStartingPawn_Prefix)
                    ),
                null);

            //Stun guns need an electric effect
            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "SoundHitPawn"), null,
    new HarmonyMethod(typeof(HarmonyPatches), "SoundHitPawnPrefix"));
            

            //Stun gun gizmos should show up
            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn_EquipmentTracker),
                    "GetGizmos"
                    ),null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(GetGizmos_PostFix)
                    ),
                null);

            //Stun gun should charge
            harmony.Patch(
            AccessTools.Method(
                typeof(Pawn),
                "Tick"
                ), null,
            new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(PawnTick_PostFix)
                ),
            null);


            // TODO
            //Testing what is going on
            harmony.Patch(
            AccessTools.Method(
                typeof(SectionLayer_FogOfWar),
                "Regenerate"
                ), 

            new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(Regenerate)
                ), null,
            null);


            harmony.Patch(
            AccessTools.Method(
                typeof(WealthWatcher),
                "CalculateWealthFloors"
                ),

            new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(CalculateWealthFloors)
                ), null,
            null);
        }

        public static bool calcWealthFloors = true;
        private static bool CalculateWealthFloors(ref float __result)
        {
            if (calcWealthFloors)
            {
                return true;
            }
            else
            {
                Log.Message("Wealth floors ignored");
                __result = 0f;
                calcWealthFloors = true;
                return false;
            }
        }

        public static bool Regenerate(SectionLayer_FogOfWar __instance)
        {
            Section section = (Section)AccessTools.Field(typeof(SectionLayer_FogOfWar), "section").GetValue(__instance);
            Map map = section.map;
            if (map.fogGrid.fogGrid == null || map.fogGrid.fogGrid.Count() == 0)
            {
                map.fogGrid.fogGrid = new bool[map.AllCells.Count()];
                Log.Message("Created new fog grid");
            }
            if (map.roofGrid == null)
            {
                map.roofGrid = new RoofGrid(map);
            }
            if (map.terrainGrid == null)
            {
                map.terrainGrid = new TerrainGrid(map);
            }
            if (map.edificeGrid.InnerArray == null || map.edificeGrid.InnerArray.Count() == 0)
            {
                map.edificeGrid = new EdificeGrid(map);
            }
            return true;
        }

        public static void PawnTick_PostFix(Pawn __instance)
        {
            if (__instance?.equipment?.Primary?.def?.defName == "RE_StunGun")
            {
                __instance.equipment.Primary.GetComp<CompStunCharge>().CompTick();
            }
        }

        public static void GetGizmos_PostFix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result)
        {
            if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
            {
                if (__instance?.Primary?.def?.defName == "RE_StunGun")
                {
                    __result = __result.Concat(__instance.Primary.GetComp<CompStunCharge>().CompGetGizmosExtra());
                }
            }
        }

        public static void SoundHitPawnPrefix(ref SoundDef __result, Verb_MeleeAttack __instance)
        {
            if (__instance.caster is Pawn pawn)
            {
                var pawn_EquipmentTracker = pawn.equipment;
                if (pawn_EquipmentTracker != null)
                {
                    //Log.Message("2");
                    var thingWithComps =
                        pawn_EquipmentTracker
                            .Primary; // (ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);

                    if (thingWithComps?.def?.defName == "RE_StunGun")
                    {
                        if (thingWithComps.TryGetComp<CompStunCharge>() is CompStunCharge charge)
                        {
                            if (charge.StoredEnergy == charge.StoredEnergyMax)
                            {
                                charge.DrainEnergy();
                                SoundDef.Named("RE_StunGun").PlayOneShot(pawn);
                                if (pawn.LastAttackedTarget.HasThing)
                                {
                                    if (pawn.LastAttackedTarget.Thing is Pawn targetPawn)
                                    {
                                        HealthUtility.AdjustSeverity(targetPawn, HediffDef.Named("RE_ShockBuildup"), 1.0f);
                                        MoteMaker.ThrowLightningGlow(targetPawn.DrawPos, pawn.Map, 5);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

            public static bool NewGeneratedStartingPawn_Prefix(ref Pawn __result)
    {
            if (Find.Scenario.name == "Resident Evil - Umbrella Corp")
            {
                var kindDef = (Rand.Value > 0.5f) ? PawnKindDef.Named("RE_UmbrellaScientist") : PawnKindDef.Named("RE_UmbrellaSecurity");
                PawnGenerationRequest request = new PawnGenerationRequest(kindDef, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, -1, true, false, false, false, true, TutorSystem.TutorialMode, 20f);
                Pawn pawn = null;
                try
                {
                    pawn = PawnGenerator.GeneratePawn(request);
                    if (pawn.kindDef.defName == "RE_UmbrellaScientist")
                    {
                        if (pawn?.story?.adulthood?.spawnCategories.Contains("RE_UmbrellaSec") == true)
                        {
                            var properScienceBackstories = BackstoryDatabase.allBackstories.Values.Where(x => x.slot == BackstorySlot.Adulthood && x.spawnCategories.Contains("RE_Umbrella"));
                            pawn.story.adulthood = properScienceBackstories.RandomElement();
                            AccessTools.Method(typeof(PawnGenerator), "GenerateBodyType").Invoke(null, new object[] { pawn });
                            AccessTools.Method(typeof(PawnGenerator), "GenerateSkills").Invoke(null, new object[] { pawn });
                            //pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                        }
                    }
                    else
                    {
                        if (pawn?.story?.adulthood?.spawnCategories.Contains("RE_Umbrella") == true)
                        {
                            var properSecurityBackstories = BackstoryDatabase.allBackstories.Values.Where(x => x.slot == BackstorySlot.Adulthood && x.spawnCategories.Contains("RE_UmbrellaSec"));
                            pawn.story.adulthood = properSecurityBackstories.RandomElement();
                            AccessTools.Method(typeof(PawnGenerator), "GenerateBodyType").Invoke(null, new object[] { pawn });
                            AccessTools.Method(typeof(PawnGenerator), "GenerateSkills").Invoke(null, new object[] { pawn });
                        }
                    }
                }
                catch (Exception arg)
                {
                    Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                    pawn = PawnGenerator.GeneratePawn(request);
                }
                pawn.relations.everSeenByPlayer = true;
                PawnComponentsUtility.AddComponentsForSpawn(pawn);
                __result = pawn;
                return false;
            }
            return true;
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

        public static bool currentlyGenerating = false;
        // RimWorld.ScenPart_PlayerPawnsArriveMethod
        public static void GenerateIntoMap(ScenPart_PlayerPawnsArriveMethod __instance, Map map)
        {
            if (currentlyGenerating) return;
            currentlyGenerating = true;
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
                currentlyGenerating = false;
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

