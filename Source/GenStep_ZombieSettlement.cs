using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    class GenStep_ZombieSettlement : GenStep_Scatterer
    {
        private static readonly IntRange SettlementSizeRange = new IntRange(90, 110);

        public override int SeedPart => 1806208471;

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map))
            {
                return false;
            }
            if (!c.Standable(map))
            {
                return false;
            }
            if (c.Roofed(map))
            {
                return false;
            }
            if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
            {
                return false;
            }
            IntRange settlementSizeRange = SettlementSizeRange;
            int min = settlementSizeRange.min;
            CellRect cellRect = new CellRect(c.x - min / 2, c.z - min / 2, min, min);
            IntVec3 size = map.Size;
            int x = size.x;
            IntVec3 size2 = map.Size;
            if (!cellRect.FullyContainedWithin(new CellRect(0, 0, x, size2.z)))
            {
                return false;
            }
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            int randomInRange = SettlementSizeRange.RandomInRange;
            int randomInRange2 = SettlementSizeRange.RandomInRange;
            CellRect rect = new CellRect(loc.x - randomInRange / 2, loc.z - randomInRange2 / 2, randomInRange, randomInRange2);
            Faction faction = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("RE_Zombies"));
            rect.ClipInsideMap(map);
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = rect;
            resolveParams.faction = faction;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push("zombiesettlement", resolveParams);
            BaseGen.Generate();
        }
    }
}
