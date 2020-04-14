using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    class GenStep_Underground : GenStep
    {
        // Token: 0x17000006 RID: 6
        // (get) Token: 0x0600001A RID: 26 RVA: 0x00002AC0 File Offset: 0x00000CC0
        public override int SeedPart
        {
            get
            {
                return 8204671;
            }
        }

        // Token: 0x0600001B RID: 27 RVA: 0x00002AD8 File Offset: 0x00000CD8
        public static ThingDef RockDefAt(IntVec3 c)
        {
            ThingDef thingDef = null;
            float num = -999999f;
            for (int i = 0; i < RockNoises.rockNoises.Count; i++)
            {
                float value = RockNoises.rockNoises[i].noise.GetValue(c);
                bool flag = value > num;
                if (flag)
                {
                    thingDef = RockNoises.rockNoises[i].rockDef;
                    num = value;
                }
            }
            bool flag2 = thingDef == null;
            if (flag2)
            {
                Log.ErrorOnce("Did not get rock def to generate at " + c, 50812, false);
                thingDef = ThingDefOf.Sandstone;
            }
            return thingDef;
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00002B78 File Offset: 0x00000D78
        public override void Generate(Map map, GenStepParams parms)
        {
            //map.regionAndRoomUpdater.Enabled = false;
            //foreach (IntVec3 intVec in map.AllCells)
            //{
            //    ThingDef def = GenStep_RocksFromGrid.RockDefAt(intVec);
            //    GenSpawn.Spawn(def, intVec, map, WipeMode.Vanish);
            //    map.roofGrid.SetRoof(intVec, RoofDefOf.RoofRockThick);
            //}
            //GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
            //float num = 16f;
            //genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num, num);
            //genStep_ScatterLumpsMineable.Generate(map, default(GenStepParams));
            //map.regionAndRoomUpdater.Enabled = true;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00002C5C File Offset: 0x00000E5C
        private bool IsNaturalRoofAt(IntVec3 c, Map map)
        {
            return c.Roofed(map) && c.GetRoof(map).isNatural;
        }

        // Token: 0x04000013 RID: 19
        private const int MinRoofedCellsPerGroup = 20;
    }
}