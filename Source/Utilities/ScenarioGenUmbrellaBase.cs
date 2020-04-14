using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public static partial class ScenarioGen
    {
        public static Pawn startPawn = null;

        public static void CreateBase(Faction fac, IntVec3 pos, Map map)
        {

            //Underground map
            var parentMap = map;
            var undergroundMap = GenUndergroundMap.Create(map);
            map = undergroundMap;
            Current.Game.CurrentMap = map;
            Current.Game.GetComponent<GameComponent_Rimhazard>().underground = undergroundMap;

            HarmonyPatches.calcWealthFloors = false;

            //Create starting area
            //var startingArea = CreateStartingArea(startingAndOptionalPawn, map);
            CreateDungeon(fac, pos, map);
        }

        public static void CreateDungeon(Faction fac, IntVec3 pos, Map map)
        {
            //Using the binary space partition method...
            var dungeonGen = new DungeonGenerator(fac, pos, map, 7);
        }


        public static CellRect CreateStartingArea(Faction pawnFaction, Map map, IntVec3 pawnPosition, CellRect rect)
        {
            RESettings.DM("Create Starting Area");


            RESettings.DM("Generate Bedrooms");
            RESettings.DM("Variable Test:");
            RESettings.DM($"{rect.CenterCell.ToString()}");
            RESettings.DM($"{map.ToString()}");
            RESettings.DM($"{pawnFaction.ToString()}");

            //Bedrooms
            CreateWallsAt(rect.CenterCell.x + -11, rect.CenterCell.z + 5, 16, false, map, ThingDefOf.Plasteel, pawnFaction, true, 13);
            SpawnBuildingAt(ThingDefOf.StandingLamp, rect.CenterCell.x - 10, rect.CenterCell.z + 6, map, pawnFaction, Rot4.North);
            SpawnBuildingAt(ThingDefOf.StandingLamp, rect.CenterCell.x + 4, rect.CenterCell.z + 6, map, pawnFaction, Rot4.North);


            RESettings.DM("Generate Beds");

            //Beds
            for (int i = 0; i < 9; i++)
            {
                if (i % 2 != 0) continue;
                SpawnBuildingAt(ThingDefOf.Bed, rect.CenterCell.x - 9 + i, rect.CenterCell.z + 9, map, pawnFaction, Rot4.South, ThingDefOf.Steel);
            }

            RESettings.DM("Generate Fab Room");

            //Fabrication room
            CreateWallsAt(rect.CenterCell.x + 5, rect.CenterCell.z + 10, 16, true, map, ThingDefOf.Plasteel, pawnFaction, true, 14);
            SpawnBuildingAt(ThingDef.Named("FabricationBench"), rect.CenterCell.x + 8, rect.CenterCell.z + 6, map, pawnFaction, Rot4.West, out Thing fabBench);
            SpawnBuildingAt(ThingDefOf.StandingLamp, rect.CenterCell.x + 6, rect.CenterCell.z + 0, map, pawnFaction, Rot4.South);
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("Stool"), ThingDefOf.Steel), fabBench.InteractionCell, map, fabBench.Rotation.Opposite);


            RESettings.DM("Generate Research Room");

            //Research Room
            CreateWallsAt(rect.CenterCell.x - 5, rect.CenterCell.z - 6, 16, false, map, ThingDefOf.Plasteel, pawnFaction, true, 1);
            SpawnBuildingAt(ThingDef.Named("HiTechResearchBench"), rect.CenterCell.x + 5, rect.CenterCell.z - 9, map, pawnFaction, Rot4.South, out Thing resBench, ThingDefOf.Plasteel);
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("Stool"), ThingDefOf.Steel), resBench.InteractionCell, map, resBench.Rotation.Opposite);

            RESettings.DM("Generate Dining Room");

            //Dining room
            CreateWallsAt(rect.CenterCell.x - 6, rect.CenterCell.z + 4, 16, true, map, ThingDefOf.Plasteel, pawnFaction, true, 1);
            SpawnBuildingAt(ThingDefOf.Table2x2c, rect.CenterCell.x - 10, rect.CenterCell.z - 3, map, pawnFaction, Rot4.North, out Thing spawnedTable, ThingDef.Named("BlocksMarble"));

            RESettings.DM("Generate Tables and Stools");

            ////Spawn a table and stools
            CellRect tableRect = spawnedTable.OccupiedRect().ExpandedBy(1);
            bool randomFlag = false;
            foreach (IntVec3 stoolSpot in tableRect.EdgeCells.InRandomOrder())
            {
                Thing spawnedStool = null;
                if (!tableRect.IsCorner(stoolSpot) && stoolSpot.Standable(map) && stoolSpot.GetEdifice(map) == null && (!randomFlag || !Rand.Bool)
                    )
                {
                    Thing stool = ThingMaker.MakeThing(ThingDef.Named("Stool"), ThingDefOf.Steel);
                    GenPlace.TryPlaceThing(stool, stoolSpot, map, ThingPlaceMode.Direct, out spawnedStool);
                    if (spawnedStool != null)
                        spawnedStool.Rotation =
                            (stoolSpot.x == tableRect.minX) ?
                            Rot4.East
                            :
                            ((stoolSpot.x == tableRect.maxX) ?
                            Rot4.West :
                            ((stoolSpot.z != tableRect.minZ) ?
                            Rot4.South : Rot4.North));
                }
            }

            RESettings.DM("Generate Crematorium");

            //For erasing some zombies ^^
            SpawnBuildingAt(ThingDef.Named("ElectricCrematorium"), rect.CenterCell.x - 8, rect.CenterCell.z - 8, map, pawnFaction, Rot4.East, ThingDefOf.Plasteel);


            RESettings.DM("Generate Geothermal Generator");

            //Geothermal Generator
            SpawnBuildingAt(ThingDefOf.GeothermalGenerator, rect.CenterCell.x - 1, rect.CenterCell.z - 1, map, pawnFaction, Rot4.North);

            RESettings.DM("Spawn Steel");

            //Steel, for making power cables
            var steelLoc = CellFinder.FindNoWipeSpawnLocNear(rect.RandomCell, map, ThingDefOf.Steel, Rot4.South);
            for (int i = 0; i < 3; i++)
            {
                Thing steelPiece = ThingMaker.MakeThing(ThingDefOf.Steel, null);
                steelPiece.stackCount = Rand.Range(15, 50);
                GenPlace.TryPlaceThing(steelPiece, steelLoc, map, ThingPlaceMode.Near);
            }

            RESettings.DM("Spawn Empty Syringes");

            //Empty syringes
            var syringesLoc = CellFinder.FindNoWipeSpawnLocNear(rect.RandomCell, map, ThingDefOf.Steel, Rot4.South);
            Thing syringePiece = ThingMaker.MakeThing(ThingDef.Named("RE_Syringe"), null);
            syringePiece.stackCount = Rand.Range(4, 6);
            GenPlace.TryPlaceThing(syringePiece, syringesLoc, map, ThingPlaceMode.Near);


            RESettings.DM("Spawn Food");

            //Food
            var foodStartPoint = CellFinder.FindNoWipeSpawnLocNear(rect.RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 2; i++)
            {
                Thing foodToEat = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
                if (GenPlace.TryPlaceThing(foodToEat, foodStartPoint, map, ThingPlaceMode.Near, out Thing foodSpawned))
                {
                    foodSpawned.stackCount = 10;
                }
            }

            RESettings.DM("Spawn Herbs");

            //Some green herbs
            var greenHerbStartPoint = CellFinder.FindNoWipeSpawnLocNear(rect.RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 3; i++)
            {
                Plant herb = (Plant)ThingMaker.MakeThing(ThingDef.Named("RE_Plant_ResidentEvilHerbGreen"));
                if (GenPlace.TryPlaceThing(herb, greenHerbStartPoint, map, ThingPlaceMode.Near, out Thing herbSpawned))
                {
                    var plantHerb = (Plant)herbSpawned;
                    plantHerb.Growth = 1.0f;
                }
            }

            RESettings.DM("Spawn Other Herbs");

            //Some other herbs.
            var otherHerbStartPoint = CellFinder.FindNoWipeSpawnLocNear(rect.RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 2; i++)
            {
                Plant herb = (Plant)ThingMaker.MakeThing(Rand.Value > 0.5f ? ThingDef.Named("RE_Plant_ResidentEvilHerbBlue") : ThingDef.Named("RE_Plant_ResidentEvilHerbRed"));
                if (GenPlace.TryPlaceThing(herb, otherHerbStartPoint, map, ThingPlaceMode.Near, out Thing herbSpawned))
                {
                    var plantHerb = (Plant)herbSpawned;
                    plantHerb.Growth = 1.0f;
                }
            }
            HarmonyPatches.calcWealthFloors = false;


            RESettings.DM("Refog");

            //Refog
            Traverse.Create(map.fogGrid).Method("SetAllFogged", new object[] { }).GetValue();


            RESettings.DM("Place pawns in area");

            //Bring in the boys
            foreach (var pawn in Find.GameInitData.startingAndOptionalPawns)
            {
                if (pawn.Spawned)
                {
                    IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(rect.CenterCell, map);
                    //CellFinder.TryFindBestPawnStandCell(pawn, out loc);
                    pawn.DeSpawn();

                    var flag = false;
                    for (int i = 0; i < GenRadial.RadialPattern.Length; i++)
                    {
                        IntVec3 intVec = loc + GenRadial.RadialPattern[i];
                        if (!PawnCanOccupy(intVec, map, pawn))
                        {
                            continue;
                        }
                        flag = true;
                        pawn.Position = intVec;
                        GenSpawn.Spawn(pawn, intVec, map);
                        break;
                    }
                    if (!flag)
                    {
                        GenSpawn.Spawn(pawn, loc, map);
                    }

                    //GenPlace.TryPlaceThing(pawn, map.Center, map, ThingPlaceMode.Near);


                    pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;

                    FloodFillerFog.FloodUnfog(pawn.Position, map);
                }
            }

            return rect;
        }


        private static bool PawnCanOccupy(IntVec3 c, Map map, Pawn pawn)
        {
            if (!c.Walkable(map))
            {
                return false;
            }
            Building edifice = c.GetEdifice(map);
            if (edifice != null)
            {
                Building_Door building_Door = edifice as Building_Door;
                if (building_Door != null && !building_Door.PawnCanOpen(pawn) && !building_Door.Open)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
