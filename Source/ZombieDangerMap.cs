using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    public class ZombieDangerMap : MapComponent
    {
        private bool firstTick = false;

        public ZombieDangerMap(Map map) : base(map)
        {

        }

        public Dictionary<Region, int> regionDangers = new Dictionary<Region, int>();

        public override void MapComponentTick()
        {
            if (!firstTick)
            {
                firstTick = true;
                foreach (Region reg in map.regionGrid.AllRegions)
                {
                    if (!regionDangers.ContainsKey(reg))
                        regionDangers.Add(reg, 1000);
                }
            }
            if (Find.TickManager.TicksGame % 260 == 0)
            {
                HashSet<Region> toUpdate = new HashSet<Region>();
                foreach (KeyValuePair<Region, int> pair in regionDangers)
                {
                    if (pair.Value > 0)
                    {
                        toUpdate.Add(pair.Key);
                    }
                }
                foreach (Region reg in toUpdate)
                {
                    regionDangers[reg] -= 500;
                }
            }
            base.MapComponentTick();
        }
    }
}
