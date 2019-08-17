using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class ZLevelTile : IExposable
    {
        int tile;
        ZLevelTracker zLevelTracker;

        public int Tile => tile;
        public ZLevelTracker ZLevelTracker => zLevelTracker;

        public ZLevelTile(int newTile, ZLevelTracker newZLevelTracker)
        {
            tile = newTile;
            zLevelTracker = newZLevelTracker;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tile, "tile");
            Scribe_Deep.Look(ref zLevelTracker, "zLevelTracker");
        }
    }
}
