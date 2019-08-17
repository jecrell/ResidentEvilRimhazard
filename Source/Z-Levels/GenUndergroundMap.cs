using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public static class GenUndergroundMap
    {
        public static Map Create(Map origin)
        {
            MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RE_Underground", true));
            mapParent.Tile = origin.Tile;
            Find.WorldObjects.Add(mapParent);
            var undergroundMapParent = (MapParent_ZLevel)mapParent;
            string seedString = Find.World.info.seedString;
            Find.World.info.seedString = new System.Random().Next(0, 2147483646).ToString();
            HarmonyPatches.calcWealthFloors = false;
            var newMap = MapGenerator.GenerateMap(origin.Size, mapParent, mapParent.MapGeneratorDef, mapParent.ExtraGenStepDefs, null);
            Find.World.info.seedString = seedString;

            var worldZLevels = Find.World.GetComponent<WorldComponent_ZLevels>();
            worldZLevels.InsertNewZLevel(origin.Tile, undergroundMapParent, true);

            Current.Game.CurrentMap = newMap;
            return newMap;
        }

    }
}
