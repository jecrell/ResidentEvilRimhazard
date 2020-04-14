using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class Building_HerbSpawner : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            var thingDef = ThingDef.Named("RE_HerbPot");
            var stuffDef = ThingDef.Named("WoodLog");

            Thing thing = (Thing)Activator.CreateInstance(thingDef.thingClass);
            thing.def = thingDef;
            thing.SetStuffDirect(stuffDef);
            thing.PostMake();

            GenSpawn.Spawn(thing, this.Position, map);
            if (thing is Building_PlantGrower pg)
            {
                var herbSpawned = ThingMaker.MakeThing(
                    new List<ThingDef> {
                        ThingDef.Named("RE_Plant_ResidentEvilHerbGreen"),
                        ThingDef.Named("RE_Plant_ResidentEvilHerbRed"),
                        ThingDef.Named("RE_Plant_ResidentEvilHerbBlue") }.RandomElement());
                pg.SetPlantDefToGrow(herbSpawned.def);
                var pHerb = (Plant)GenSpawn.Spawn(herbSpawned, this.Position, map);
                    pHerb.Growth = 1f;
            }
        }
    }
}
