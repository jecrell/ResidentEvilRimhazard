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

        private List<int> list2;

        private List<MapParent_ZLevel> list3;

        public ZLevelTracker(MapParent_ZLevel mp, bool downwards)
        {
            AddLayer(mp, downwards);
        }

        public int NumOfLevels => layersState?.Keys?.Count() + 1 ?? 1;
                
        public int NextNewLevelDown()
        {
            int num = 0;
            while (this.layersState.ContainsKey(num))
                num--;
            return num;
        }

        public int NextNewLevelUp()
        {
            int num = 0;
            while (this.layersState.ContainsKey(num))
                num++;
            return num;
        }


        public void AddLayer(MapParent_ZLevel mp, bool downwards)
        {
            int nextEmptyLayer = (downwards) ? NextNewLevelDown() : NextNewLevelUp();
            this.layersState.Add(nextEmptyLayer, mp);
            mp.SetZ(nextEmptyLayer);
        }


        public int NextLayerUp()
        {
            int num = 0;
            while (this.layersState.ContainsKey(num))
                num++;
            return num - 1;
        }


        public void DestroyLevel(MapParent_ZLevel layer)
        {
            int z = layer.Z;
            this.layersState[z] = null;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<int, MapParent_ZLevel>(ref this.layersState, "layers", LookMode.Value, LookMode.Reference, ref this.list2, ref this.list3);
        }
        
    }
}
