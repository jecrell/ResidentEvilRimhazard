using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace RERimhazard
{
    public static partial class ScenarioGen
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
            var buildingLoc = new IntVec3(x, 0, z);
            //var buildingLoc = new IntVec3(map.Center.x + x, 0, map.Center.z + z);
            GenSpawn.Spawn(building, buildingLoc, map, dir);
            building.SetFaction(fac);
        }


        public static void SpawnBuildingAt(ThingDef def, int x, int z, Map map, Faction fac, Rot4 dir, out Thing building, ThingDef stuff = null)
        {
            building = ThingMaker.MakeThing(def, stuff);
            var buildingLoc = new IntVec3(x, 0, z);
            //var buildingLoc = new IntVec3(map.Center.x + x, 0, map.Center.z + z);
            GenSpawn.Spawn(building, buildingLoc, map, dir);
            building.SetFaction(fac);
        }

        public static void CreateWallsAt(int startX, int startZ, int numOfWalls, bool isVertical, Map map, ThingDef stuff, Faction fac, bool hasDoor = true, int createDoorAtIndex = 12)
        {
            //var wallStart = new IntVec3(map.Center.x - 10, 0, map.Center.z + 4);
            var wallStart = new IntVec3(startX, 0, startZ);
            //var wallStart = new IntVec3(map.Center.x + startX, 0, map.Center.z + startZ);
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
        


        public static ThingDef RockDefAt(IntVec3 c, Map map, List<RockNoises.RockNoise> rockNoises)
        {
            ThingDef thingDef = null;
            float num = -999999f;

            for (int i = 0; i < rockNoises.Count; i++)
            {
                float value = rockNoises[i].noise.GetValue(c);
                if (value > num)
                {
                    thingDef = rockNoises[i].rockDef;
                    num = value;
                }
            }
            if (thingDef == null)
            {
                thingDef = ThingDefOf.Granite;
            }
            return thingDef;
        }


        public static TerrainDef lastTerrain = TerrainDefOf.Soil;
        public static void UndergroundRockAdder(Map origin, Map newMap)
        {

            var rockNoises = new List<RockNoises.RockNoise>();
            foreach (ThingDef item in Find.World.NaturalRockTypesIn(origin.Tile))
            {
                RockNoises.RockNoise rockNoise = new RockNoises.RockNoise();
                rockNoise.rockDef = item;
                rockNoise.noise = new Verse.Noise.Perlin(0.004999999888241291, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.Medium);
                rockNoises.Add(rockNoise);
                NoiseDebugUI.StoreNoiseRender(rockNoise.noise, rockNoise.rockDef + " score", origin.Size.ToIntVec2);
            }

            foreach (IntVec3 cell in origin.AllCells)
            {
                var originTerrain = cell.GetTerrain(origin);
                var newTerrain = lastTerrain;
                if (!originTerrain.BuildableByPlayer
                    && originTerrain.defName != "BrokenAsphalt"
                    && !originTerrain.IsWater)
                {
                    newTerrain = originTerrain;
                    lastTerrain = originTerrain;
                }
                if (originTerrain.defName != "WaterDeep")
                {
                    newMap.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
                    newMap.terrainGrid.SetTerrain(cell, newTerrain);
                    GenSpawn.Spawn(RockDefAt(cell, origin, rockNoises), cell, newMap);
                }
                else
                {
                    newMap.terrainGrid.SetTerrain(cell, TerrainDefOf.WaterDeep);
                }
            }
        }

            public static void CreateOutpost(Faction fac, IntVec3 pos, Map map)
        {
            
            //Place a table
            Thing spawnedTable = null;
            Thing table = ThingMaker.MakeThing(ThingDefOf.Table2x2c, ThingDefOf.WoodLog);
            var tableLoc = CellFinder.FindNoWipeSpawnLocNear(pos.GetRegion(map).RandomCell, map, ThingDefOf.Table2x2c, Rot4.South);
            table.SetFaction(fac);
                if (GenPlace.TryPlaceThing(table, tableLoc, map, ThingPlaceMode.Near, out spawnedTable))
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
            campfire.SetFaction(fac);
            var campfireLoc = CellFinder.FindNoWipeSpawnLocNear(pos.GetRegion(map).RandomCell, map, ThingDefOf.Campfire, Rot4.South);
            GenPlace.TryPlaceThing(campfire, campfireLoc, map, ThingPlaceMode.Near);

            //Food
            var foodStartPoint = CellFinder.FindNoWipeSpawnLocNear(pos.GetRegion(map).RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 2; i++)
            {
                Thing foodToEat = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
                if (GenPlace.TryPlaceThing(foodToEat, foodStartPoint, map, ThingPlaceMode.Near, out Thing foodSpawned))
                {
                    foodSpawned.stackCount = 10;
                }
            }

            //Some green herbs
            var greenHerbStartPoint = CellFinder.FindNoWipeSpawnLocNear(pos.GetRegion(map).RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 3; i++)
            {
                Plant herb = (Plant)ThingMaker.MakeThing(ThingDef.Named("RE_Plant_ResidentEvilHerbGreen"));
                if (GenPlace.TryPlaceThing(herb, greenHerbStartPoint, map, ThingPlaceMode.Near, out Thing herbSpawned))
                {
                    var plantHerb = (Plant)herbSpawned;
                    plantHerb.Growth = 1.0f;
                }
            }

            //Some other herbs.
            var otherHerbStartPoint = CellFinder.FindNoWipeSpawnLocNear(pos.GetRegion(map).RandomCell, map, ThingDefOf.MealSurvivalPack, Rot4.South);
            for (int i = 0; i < 2; i++)
            {
                Plant herb = (Plant)ThingMaker.MakeThing(Rand.Value > 0.5f ? ThingDef.Named("RE_Plant_ResidentEvilHerbBlue") : ThingDef.Named("RE_Plant_ResidentEvilHerbRed"));
                if (GenPlace.TryPlaceThing(herb, otherHerbStartPoint, map, ThingPlaceMode.Near, out Thing herbSpawned))
                {
                    var plantHerb = (Plant)herbSpawned;
                    plantHerb.Growth = 1.0f;
                }
            }

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
            //Current.Game.AddMap(map);
            //Current.Game.CurrentMap = map;
        }
    }
}