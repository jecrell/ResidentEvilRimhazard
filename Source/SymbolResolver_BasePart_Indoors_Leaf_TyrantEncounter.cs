using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    class SymbolResolver_BasePart_Indoors_Leaf_TyrantEncounter : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            if (!base.CanResolve(rp) || REDataCache.TyrantEncounterGenerated)
            {
                return false;
            }
            return true;
        }

        public override void Resolve(ResolveParams rp)
        {
            rp.floorDef = TerrainDefOf.MetalTile;
            BaseGen.symbolStack.Push("reTyrantEncounter", rp);
            REDataCache.TyrantEncounterGenerated = true;
        }
    }

}
