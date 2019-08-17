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

        public static void CreateBase(Pawn startingAndOptionalPawn, Map map)
        {
            var fac = startingAndOptionalPawn.Faction;

            //Underground map
            var parentMap = map;
            var undergroundMap = GenUndergroundMap.Create(map);
            map = undergroundMap;
            Current.Game.CurrentMap = map;

            HarmonyPatches.calcWealthFloors = false;

            //Clear base space
            CellRect rect = new CellRect(map.Center.x - 12, map.Center.z - 12, 24, 24);
            foreach (IntVec3 cell in rect)
            {
                if (cell.GetEdifice(map) is Thing t)
                {
                    t.Destroy();
                }
                map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
            }
            foreach (IntVec3 cell in rect.EdgeCells)
            {
                var wall = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Plasteel);
                GenSpawn.Spawn(wall, cell, map);
            }

            //Bedrooms
            CreateWallsAt(-11, 5, 16, false, map, ThingDefOf.Plasteel, fac, true, 13);
            SpawnBuildingAt(ThingDefOf.StandingLamp, -10, 6, map, fac, Rot4.North);
            SpawnBuildingAt(ThingDefOf.StandingLamp, 4, 6, map, fac, Rot4.North);

            //Beds
            for (int i = 0; i < 9; i++)
            {
                if (i % 2 != 0) continue;
                SpawnBuildingAt(ThingDefOf.Bed, -9 + i, 9, map, fac, Rot4.South, ThingDefOf.Steel);
            }

            //Fabrication room
            CreateWallsAt(5, 10, 16, true, map, ThingDefOf.Plasteel, fac, true, 14);
            SpawnBuildingAt(ThingDef.Named("FabricationBench"), 8, 6, map, fac, Rot4.West, out Thing fabBench);
            SpawnBuildingAt(ThingDefOf.StandingLamp, 6, 0, map, fac, Rot4.South);
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("Stool"), ThingDefOf.Steel), fabBench.InteractionCell, map, fabBench.Rotation.Opposite);

            //Research Room
            CreateWallsAt(-5, -6, 16, false, map, ThingDefOf.Plasteel, fac, true, 1);
            SpawnBuildingAt(ThingDef.Named("HiTechResearchBench"), 5, -9, map, fac, Rot4.South, out Thing resBench);
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("Stool"), ThingDefOf.Steel), resBench.InteractionCell, map, resBench.Rotation.Opposite);

            //Dining room
            CreateWallsAt(-6, 4, 16, true, map, ThingDefOf.Plasteel, fac, true, 1);
            SpawnBuildingAt(ThingDefOf.Table2x2c, -10, -3, map, fac, Rot4.North, out Thing spawnedTable, ThingDef.Named("BlocksMarble"));

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

            //For erasing some zombies ^^
            SpawnBuildingAt(ThingDef.Named("ElectricCrematorium"), -8, -8, map, fac, Rot4.East);

            //Geothermal Generator
            SpawnBuildingAt(ThingDefOf.GeothermalGenerator, -1, -1, map, fac, Rot4.North);


            //Steel, for making power cables
            var steelLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, ThingDefOf.Steel, Rot4.South);
            for (int i = 0; i < 3; i++)
            {
                Thing steelPiece = ThingMaker.MakeThing(ThingDefOf.Steel, null);
                steelPiece.stackCount = Rand.Range(15, 50);
                GenPlace.TryPlaceThing(steelPiece, steelLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near);
            }


            //Empty syringes
            var syringesLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, ThingDefOf.Steel, Rot4.South);
            Thing syringePiece = ThingMaker.MakeThing(ThingDef.Named("RE_Syringe"), null);
            syringePiece.stackCount = Rand.Range(4, 6);
            GenPlace.TryPlaceThing(syringePiece, syringesLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near);



            //Food
            var foodStartPoint = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 2; i++)
            {
                Thing foodToEat = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
                if (GenPlace.TryPlaceThing(foodToEat, foodStartPoint, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near, out Thing foodSpawned))
                {
                    foodSpawned.stackCount = 10;
                }
            }

            //Some green herbs
            var greenHerbStartPoint = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 3; i++)
            {
                Plant herb = (Plant)ThingMaker.MakeThing(ThingDef.Named("RE_Plant_ResidentEvilHerbGreen"));
                if (GenPlace.TryPlaceThing(herb, greenHerbStartPoint, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near, out Thing herbSpawned))
                {
                    var plantHerb = (Plant)herbSpawned;
                    plantHerb.Growth = 1.0f;
                }
            }

            //Some other herbs.
            var otherHerbStartPoint = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 2; i++)
            {
                Plant herb = (Plant)ThingMaker.MakeThing(Rand.Value > 0.5f ? ThingDef.Named("RE_Plant_ResidentEvilHerbBlue") : ThingDef.Named("RE_Plant_ResidentEvilHerbRed"));
                if (GenPlace.TryPlaceThing(herb, otherHerbStartPoint, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near, out Thing herbSpawned))
                {
                    var plantHerb = (Plant)herbSpawned;
                    plantHerb.Growth = 1.0f;
                }
            }
            HarmonyPatches.calcWealthFloors = false;

            //Bring in the boys
            foreach (var pawn in Find.GameInitData.startingAndOptionalPawns)
            {
                if (pawn.Spawned)
                {
                    var loc = pawn.Position;
                    pawn.DeSpawn();
                    GenSpawn.Spawn(pawn, loc, map);
                    //GenPlace.TryPlaceThing(pawn, map.Center, map, ThingPlaceMode.Near);
                }
            }
        }


    }
}
