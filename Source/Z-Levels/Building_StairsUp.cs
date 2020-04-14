using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public class Building_StairsUp : Building
    {
        public void ClearSpot(IntVec3 spot, Map map)
        {
            var tmpThingsToDestroy = new List<Thing>(spot.GetThingList(map));
            for (int j = 0; j < tmpThingsToDestroy.Count; j++)
            {
                if (tmpThingsToDestroy[j].def.destroyable)
                {
                    tmpThingsToDestroy[j].Destroy();
                }
            }
            map.roofGrid.SetRoof(spot, null);
        }

        public void MakeSmallRoom(IntVec3 pos, Map map)
        {
            /////////////
            /// W W W W W
            /// W F F F W
            /// W F S F W
            /// W F F F W
            /// W D W W W
            /////////////

            var innerCircle = new List<IntVec3>
            {
                new IntVec3(pos.x -1, pos.y, pos.z -1),
                new IntVec3(pos.x -1, pos.y, pos.z),
                new IntVec3(pos.x -1, pos.y, pos.z + 1),
                new IntVec3(pos.x, pos.y, pos.z - 1),
                new IntVec3(pos.x, pos.y, pos.z),
                new IntVec3(pos.x, pos.y, pos.z + 1),
                new IntVec3(pos.x + 1, pos.y, pos.z - 1),
                new IntVec3(pos.x + 1, pos.y, pos.z),
                new IntVec3(pos.x + 1, pos.y, pos.z + 1),
            };

            var outerCircle = new List<IntVec3>
            {

                new IntVec3(pos.x - 2, pos.y, pos.z + 2),
                new IntVec3(pos.x - 2, pos.y, pos.z + 1),
                new IntVec3(pos.x - 2, pos.y, pos.z),
                new IntVec3(pos.x - 2, pos.y, pos.z - 1),
                new IntVec3(pos.x - 2, pos.y, pos.z - 2),


                new IntVec3(pos.x - 1, pos.y, pos.z + 2),
                new IntVec3(pos.x - 1, pos.y, pos.z - 2),
                new IntVec3(pos.x, pos.y, pos.z + 2),
                new IntVec3(pos.x, pos.y, pos.z -2),
                new IntVec3(pos.x + 1, pos.y, pos.z + 2),
                new IntVec3(pos.x + 1, pos.y, pos.z - 2),



                new IntVec3(pos.x + 2, pos.y, pos.z + 2),
                new IntVec3(pos.x + 2, pos.y, pos.z + 1),
                new IntVec3(pos.x + 2, pos.y, pos.z),
                new IntVec3(pos.x + 2, pos.y, pos.z - 1),
                new IntVec3(pos.x + 2, pos.y, pos.z - 2),

            };

            foreach (IntVec3 innerCell in innerCircle)
            {
                if (innerCell.IsValid)
                {
                    ClearSpot(innerCell, map);
                    map.terrainGrid.SetTerrain(innerCell, TerrainDefOf.WoodPlankFloor);
                }
            }



            var doorLoc = new IntVec3(pos.x, pos.y, pos.z - 2);
            foreach (IntVec3 outerCell in outerCircle)
            {
                if (outerCell.IsValid)
                {
                    ClearSpot(outerCell, map);
                    map.terrainGrid.SetTerrain(outerCell, TerrainDefOf.WoodPlankFloor);
                    Thing thing;
                    if (outerCell != doorLoc)
                    {
                        thing = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.WoodLog);
                    }
                    else
                    {
                        thing = ThingMaker.MakeThing(ThingDefOf.Door, ThingDefOf.WoodLog);
                    }
                    GenPlace.TryPlaceThing(thing, outerCell, map, ThingPlaceMode.Direct);

                }
            }

            var stairsDown = ThingMaker.MakeThing(ThingDef.Named("RE_StairsDown"), ThingDefOf.WoodLog);
            GenPlace.TryPlaceThing(stairsDown, pos, map, ThingPlaceMode.Direct);
            
        }

        private bool spawnedOppositeStairs = false;
        public override void TickRare()
        {
            base.TickRare();

            if (!spawnedOppositeStairs && Find.TickManager.TicksGame > 100)
            {
                if (Find.World.GetComponent<WorldComponent_ZLevels>() is WorldComponent_ZLevels zLvls && zLvls.HasZLevelsAbove(Map.Parent))
                {
                    spawnedOppositeStairs = true;
                    var upMap = zLvls.GetAboveMap(Map.Parent);
                    if (upMap != null)
                    {
                        if (this.PositionHeld.GetRoom(upMap) is Room r && (r.PsychologicallyOutdoors || !this.PositionHeld.Roofed(upMap)))
                        {
                            MakeSmallRoom(this.PositionHeld, upMap);
                        }
                    }
                }
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
            {
                yield return opt;
            }

            if (Find.World.GetComponent<WorldComponent_ZLevels>().HasZLevelsAbove(Map.Parent))
            {
                yield return new FloatMenuOption(
                    "Go up",
                    () =>
                    {
                        Job job = new Job(DefDatabase<JobDef>.GetNamed("RE_GoToStairs"), this);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                    }
                    );
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spawnedOppositeStairs, "spawnedOppositeStairs");
        }
    }
}
