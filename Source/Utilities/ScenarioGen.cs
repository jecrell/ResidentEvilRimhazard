using RimWorld;
using System;
using System.Linq;
using Verse;

namespace RERimhazard
{
    public static class ScenarioGen
    {
        public static void CreateBeds(Pawn startingAndOptionalPawn, Map map, ThingDef bedType, ThingDef stuffType)
        {
            //Place a sleeping bag on the ground near them.
            Thing sleepingBag = ThingMaker.MakeThing(bedType, stuffType);
            sleepingBag.Rotation = Rot4.Random;
            for (int i = 0; i < 30; i++)
            {
                var randomLocation = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.PositionHeld, map, bedType, sleepingBag.Rotation);
                if (GenPlace.TryPlaceThing(sleepingBag, randomLocation, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near, out Thing bedThing))
                {
                    sleepingBag.SetFaction(startingAndOptionalPawn.Faction);
                    break;
                }
            }
            
        }

        public static void SpawnBuildingAt(ThingDef def, int x, int z, Map map, Faction fac, Rot4 dir, ThingDef stuff = null)
        {
            Thing building = ThingMaker.MakeThing(def, stuff);
            var buildingLoc = new IntVec3(map.Center.x + x, 0, map.Center.z + z);
            GenSpawn.Spawn(building, buildingLoc, map, dir);
            building.SetFaction(fac);
        }


        public static void SpawnBuildingAt(ThingDef def, int x, int z, Map map, Faction fac, Rot4 dir, out Thing building, ThingDef stuff = null)
        {
            building = ThingMaker.MakeThing(def, stuff);
            var buildingLoc = new IntVec3(map.Center.x + x, 0, map.Center.z + z);
            GenSpawn.Spawn(building, buildingLoc, map, dir);
            building.SetFaction(fac);
        }

        public static void CreateWallsAt(int startX, int startZ, int numOfWalls, bool isVertical, Map map, ThingDef stuff, Faction fac, bool hasDoor = true, int createDoorAtIndex = 12)
        {
            //var wallStart = new IntVec3(map.Center.x - 10, 0, map.Center.z + 4);
            var wallStart = new IntVec3(map.Center.x + startX, 0, map.Center.z + startZ);
            for (int i = 0; i < numOfWalls; i++)
            {
                Thing wall = (i == createDoorAtIndex) ? ThingMaker.MakeThing(ThingDefOf.Door, stuff) : ThingMaker.MakeThing(ThingDefOf.Wall, stuff);
                GenPlace.TryPlaceThing(wall, 
                    isVertical ? 
                        new IntVec3(wallStart.x, 0, wallStart.z - i) :
                        new IntVec3(wallStart.x + i, 0, wallStart.z),
                    map, ThingPlaceMode.Direct);
                wall.SetFaction(fac);
            }
        }
        
        public static void CreateBase(Pawn startingAndOptionalPawn, Map map)
        {
            var fac = startingAndOptionalPawn.Faction;

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
        }

            public static void CreateOutpost(Pawn startingAndOptionalPawn, Map map)
        {
            
            //Place a table
            Thing spawnedTable = null;
            Thing table = ThingMaker.MakeThing(ThingDefOf.Table2x2c, ThingDefOf.WoodLog);
            var tableLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, ThingDefOf.Table2x2c, Rot4.South);
            table.SetFaction(startingAndOptionalPawn.Faction);
                if (GenPlace.TryPlaceThing(table, tableLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near, out spawnedTable))
                {
                }
           
            //Spawn a table and stools
            CellRect tableRect = spawnedTable.OccupiedRect().ExpandedBy(1);
            bool randomFlag = false;
            foreach (IntVec3 stoolSpot in tableRect.EdgeCells.InRandomOrder())
            {
                Thing spawnedStool = null;
                if (!tableRect.IsCorner(stoolSpot) && stoolSpot.Standable(map) && stoolSpot.GetEdifice(map) == null && (!randomFlag || !Rand.Bool)
                    )
                {
                    Thing stool = ThingMaker.MakeThing(ThingDef.Named("Stool"), ThingDefOf.WoodLog);
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

            //Campfire
            Thing campfire = ThingMaker.MakeThing(ThingDefOf.Campfire);
            campfire.SetFaction(startingAndOptionalPawn.Faction);
            var campfireLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, ThingDefOf.Campfire, Rot4.South);
            GenPlace.TryPlaceThing(campfire, campfireLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near);

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
        }
    }
}