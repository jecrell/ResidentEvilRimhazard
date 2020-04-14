using HarmonyLib;
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
        private int nextSeries = 0;

        Dictionary<int, ZLevelTracker> tilesWithZLevels = new Dictionary<int, ZLevelTracker>();

        Dictionary<Map, int> mapIDs = new Dictionary<Map, int>();

        private List<Map> maps;
        private List<int> IDs;

        private List<int> blah;

        private List<ZLevelTracker> blah2;

        Dictionary<Pawn, TargetInfo> pawnsToTeleport = new Dictionary<Pawn, TargetInfo>();

        public WorldComponent_ZLevels(World world) : base(world)
        {

        }

        public void AddTeleportJob(Pawn pawn, TargetInfo target)
        {
            pawnsToTeleport.Add(pawn, target);

        }

        private bool isTeleporting = false;

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % 100 == 0 && !isTeleporting)
            {
                isTeleporting = true;
                if (pawnsToTeleport != null && pawnsToTeleport?.Count() > 0)
                {
                    foreach (var teleportPawn in pawnsToTeleport)
                    {
                        teleportPawn.Key.DeSpawn();
                        GenPlace.TryPlaceThing(teleportPawn.Key, teleportPawn.Value.Cell, teleportPawn.Value.Map, ThingPlaceMode.Near);
                        AccessTools.Method(typeof(FogGrid), "FloodUnfogAdjacent").Invoke(teleportPawn.Value.Map.fogGrid, new object[] { teleportPawn.Key.PositionHeld });
                    }
                }
                pawnsToTeleport.Clear();
                isTeleporting = false;

            }

        }


        public int NumOfZLevelsAtMap(Map map)
        {
            if (mapIDs.ContainsKey(map))
            {
                var id = mapIDs[map];
                return tilesWithZLevels[id].NumOfLevels;
            }
            return 1;
        }

        public Map GetAboveMap(MapParent mapParent)
        {
            if (mapIDs.ContainsKey(mapParent.Map))
            {
                var series = mapIDs[mapParent.Map];
                return tilesWithZLevels[series].NextMapUp(mapParent);
            }
            Log.Message("No above map found");
            return null;
        }

        public Map GetBelowMap(MapParent mapParent)
        {
            if (mapIDs.ContainsKey(mapParent.Map))
            {
                var series = mapIDs[mapParent.Map];
                return tilesWithZLevels[series].NextMapDown(mapParent);
            }
            Log.Message("No below map found");
            return null;
        }

        public bool HasZLevelsBelow(int tile, MapParent mapParent)
        {
            if (mapIDs.ContainsKey(mapParent.Map))
            {
                var series = mapIDs[mapParent.Map];
                return tilesWithZLevels[series].HasZLevelBelow(mapParent);
            }
            return false;
        }

        public bool HasZLevelsAbove(MapParent mapParent)
        {
            if (mapIDs.ContainsKey(mapParent.Map))
            {
                var series = mapIDs[mapParent.Map];
                return tilesWithZLevels[series].HasZLevelAbove(mapParent);
            }
            return false;
        }

        public void InsertNewZLevel(Map zeroMap, MapParent zeroLevel, MapParent_ZLevel newLevel, bool downwards = true)
        {
            if (mapIDs.ContainsKey(zeroMap))
            {
                tilesWithZLevels[mapIDs[zeroMap]].AddLayer(newLevel, downwards);
                Log.Message(mapIDs[zeroMap] + " Series, Z Levels: " + NumOfZLevelsAtMap(zeroMap));
                return;
            }
            mapIDs.Add(zeroMap, nextSeries);
            mapIDs.Add(newLevel.Map, nextSeries);
            tilesWithZLevels.Add(nextSeries, new ZLevelTracker(zeroMap, zeroLevel, newLevel, downwards));
            Log.Message(nextSeries + " Series, Z Levels: " + NumOfZLevelsAtMap(zeroMap));
            nextSeries++;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.tilesWithZLevels, "tilesWithZLevels", LookMode.Value, LookMode.Deep, ref blah, ref blah2);
            Scribe_Values.Look(ref this.nextSeries, "nextSeries");
        }
    }
}
