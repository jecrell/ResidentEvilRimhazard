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
            sleepingBag.SetFaction(startingAndOptionalPawn.Faction);
            for (int i = 0; i < 30; i++)
            {
                var randomLocation = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.PositionHeld, map, bedType, sleepingBag.Rotation);
                if (GenPlace.TryPlaceThing(sleepingBag, randomLocation, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near, out Thing bedThing))
                {
                    break;
                }
            }
        }

        public static void CreateBase(Pawn startingAndOptionalPawn, Map map)
        {
            //Place a table
            Thing spawnedTable = null;
            Thing table = ThingMaker.MakeThing(ThingDefOf.Table2x2c, ThingDefOf.Steel);
            table.SetFaction(startingAndOptionalPawn.Faction);

            var randomLocation = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.PositionHeld, map, table.def, table.Rotation);
            GenPlace.TryPlaceThing(table, randomLocation, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near, out spawnedTable);

            //Spawn a table and stools
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

            //Geothermal Generator
            for (int i = 0; i < 30; i++)
            {
                Thing geothermGen = ThingMaker.MakeThing(ThingDefOf.GeothermalGenerator, null);
                geothermGen.SetFaction(startingAndOptionalPawn.Faction);
                var geothermGenLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, geothermGen.def, geothermGen.Rotation);
                if (GenPlace.TryPlaceThing(geothermGen, geothermGenLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near))
                    break;
            }

            //Light source
            Thing light = ThingMaker.MakeThing(ThingDefOf.StandingLamp, null);
            light.SetFaction(startingAndOptionalPawn.Faction);
            var lightLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, light.def, light.Rotation);
            GenPlace.TryPlaceThing(light, lightLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near);

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


            //Hi-Tech Research bench
            for (int i = 0; i < 30; i++)
            {
                Thing bench = ThingMaker.MakeThing(ThingDef.Named("HiTechResearchBench"));
                bench.SetFaction(startingAndOptionalPawn.Faction);
                var hitechBenchLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, bench.def, bench.Rotation);
                if (GenPlace.TryPlaceThing(bench, hitechBenchLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near))
                    break;
            }


            //Fabrication Bench
            for (int i = 0; i < 30; i ++)
            {
                Thing fabBench = ThingMaker.MakeThing(ThingDef.Named("FabricationBench"));
                fabBench.SetFaction(startingAndOptionalPawn.Faction);
                var fabBenchLoc = CellFinder.FindNoWipeSpawnLocNear(startingAndOptionalPawn.GetRegion().RandomCell, map, fabBench.def, fabBench.Rotation);
                if (GenPlace.TryPlaceThing(fabBench, fabBenchLoc, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near))
                    break;
            }

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