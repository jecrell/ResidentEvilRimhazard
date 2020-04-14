using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class ZLevelTracker : IExposable
    {
        public Dictionary<int, MapParent_ZLevel> layersState = new Dictionary<int, MapParent_ZLevel>();

        public MapParent zeroLevel;

        public Map zeroLevelMap;

        private List<int> list2;

        private List<MapParent_ZLevel> list3;

        public ZLevelTracker(Map newZeroLevlMap, MapParent newZeroLevel, MapParent_ZLevel mp, bool downwards)
        {
            layersState.Add(0, newZeroLevel as MapParent_ZLevel);
            zeroLevelMap = newZeroLevlMap;
            zeroLevel = newZeroLevel;

            //Refog zero layer
            Traverse.Create(newZeroLevlMap.fogGrid).Method("SetAllFogged", new object[] { }).GetValue();

            AddLayer(mp, downwards);
        }

        public void ResolveZeroLevel()
        {
            if (zeroLevelMap != null && zeroLevel == null)
            {
                zeroLevel = zeroLevelMap.Parent;
                layersState.Add(0, zeroLevel as MapParent_ZLevel);
                layersState[0].SetZ(0);
            }
        }

        public bool HasZLevelBelow(MapParent current)
        {
            ResolveZeroLevel();
            if (layersState == null || layersState.Count() == 0)
                return false;

            var pair = layersState.FirstOrDefault(x => x.Value == current);
            if (layersState.ContainsKey(pair.Key - 1))
                return true;
            return false;
        }

        public bool HasZLevelAbove(MapParent current)
        {
            ResolveZeroLevel();
            if (layersState == null || layersState.Count() == 0)
                return false;

            var pair = layersState.FirstOrDefault(x => x.Value == current);
            if (layersState.ContainsKey(pair.Key + 1))
                return true;
            return false;
        }

        public int NumOfLevels => layersState?.Keys?.Count() ?? 1;
                
        public int NextNewLevelDown()
        {
            int num = -1;
            while (this.layersState.ContainsKey(num))
                num--;
            return num;
        }

        public int NextNewLevelUp()
        {
            int num = 1;
            while (this.layersState.ContainsKey(num))
                num++;
            return num;
        }


        public void AddLayer(MapParent_ZLevel mp, bool downwards)
        {
            int nextEmptyLayer = (downwards) ? NextNewLevelDown() : NextNewLevelUp();
            mp.SetZ(nextEmptyLayer);
            this.layersState.Add(nextEmptyLayer, mp);

            //Refog the map
            if (mp.HasMap && mp.Map != null)
                Traverse.Create(mp.Map.fogGrid).Method("SetAllFogged", new object[] { }).GetValue();

        }

        public Map NextMapUp (MapParent mp)
        {
            if (mp == zeroLevel)
            {
                return layersState[1].Map;
            }

            var zMap = mp as MapParent_ZLevel;
            var zLevel = zMap?.Z ?? -1;

            if (zMap == null || zLevel == -1)
                return zeroLevelMap;

            foreach (var state in layersState)
                Log.Message(state.ToString());
            return layersState[zLevel + 1].Map;
        }

        public Map NextMapDown(MapParent mp)
        {
            if (mp == zeroLevel)
            {
                return layersState[-1].Map;
            }

            var zMap = mp as MapParent_ZLevel;
            var zLevel = zMap?.Z ?? 1;

            if (zMap == null || zLevel == 1)
                return zeroLevelMap;

            foreach (var state in layersState)
                Log.Message(state.ToString());
            return layersState[zLevel - 1].Map;
        }

        public void DestroyLevel(MapParent_ZLevel layer)
        {
            int z = layer.Z;
            this.layersState[z] = null;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<int, MapParent_ZLevel>(ref this.layersState, "layers", LookMode.Value, LookMode.Reference, ref this.list2, ref this.list3);
            Scribe_References.Look(ref this.zeroLevel, "zeroLevel");
            Scribe_References.Look(ref this.zeroLevelMap, "zeroLevelMap");

        }
        
    }
}
