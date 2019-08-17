using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class WorldComponent_ZLevels : WorldComponent
    {
        Dictionary<int, ZLevelTracker> tilesWithZLevels = new Dictionary<int, ZLevelTracker>();

        public WorldComponent_ZLevels(World world) : base(world)
        {

        }
        
        public int NumOfZLevelsAtTile(int tile)
        {
            if (tilesWithZLevels.ContainsKey(tile))
            {
                return tilesWithZLevels[tile].NumOfLevels;
            }
            return 1;
        }

        public void InsertNewZLevel(int tile, MapParent_ZLevel mapParent, bool downwards = true)
        {
            if (tilesWithZLevels.ContainsKey(tile))
                tilesWithZLevels[tile].AddLayer(mapParent, downwards);
            else
                tilesWithZLevels.Add(tile, new ZLevelTracker(mapParent, downwards));
            Log.Message(tile + " Tile, Z Levels: " + NumOfZLevelsAtTile(tile));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.tilesWithZLevels, "tilesWithZLevels", LookMode.Value, LookMode.Deep);
        }
    }
}
