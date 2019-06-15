using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    class SymbolResolver_BasePart_Indoors_Leaf_GEncounter : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            if (!base.CanResolve(rp) || REDataCache.GEncounterGenerated)
            {
                return false;
            }
            return true;
        }

        public override void Resolve(ResolveParams rp)
        {
            rp.floorDef = TerrainDef.Named("SterileTile");
            BaseGen.symbolStack.Push("reGEncounter", rp);
            REDataCache.GEncounterGenerated = true;
        }
    }

}
