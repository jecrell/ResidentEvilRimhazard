using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public static class REDataCache
    {
        public static Dictionary<Map, Thing> brainChips = new Dictionary<Map, Thing>();

        public static Thing GetFirstBrainChip(Map map)
        {
            if (!brainChips.ContainsKey(map))
            {
                brainChips.Add(map, null);
            }
            if (brainChips[map] == null || !brainChips[map].Spawned)
            {
                var brainChip = map.listerThings.AllThings.FirstOrDefault(x => x.def.defName == "RE_BrainChip" && x.Spawned);
                brainChips[map] = brainChip;
            }
            return brainChips[map];
        }

        public static void ClearBrainChipCache(Map map)
        {
            if (!brainChips.ContainsKey(map))
            {
                return;
            }
            brainChips.Remove(map);
        }
    }
}
