using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class Building_CageDog : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            bool doorSpawned = false;
            foreach (var cell in this.OccupiedRect().ContractedBy(1).EdgeCells)
            {
                if (!this.OccupiedRect().ContractedBy(1).Corners.Contains(cell))
                {
                    if (!doorSpawned)
                    {
                        doorSpawned = true;
                        var door = ThingMaker.MakeThing(ThingDefOf.Door, ThingDefOf.Steel);
                        GenPlace.TryPlaceThing(door, cell, map, ThingPlaceMode.Near);
                        continue;
                    }
                }
                var buildingSpawned = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Steel);
                GenPlace.TryPlaceThing(buildingSpawned, cell, map, ThingPlaceMode.Near);
            }
            this.Destroy();

        }
    }
}
