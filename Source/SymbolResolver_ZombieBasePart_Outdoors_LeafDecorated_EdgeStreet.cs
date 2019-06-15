using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class SymbolResolver_ZombieBasePart_Outdoors_LeafDecorated_EdgeStreet : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            ResolveParams resolveParams = rp;
            //TerrainDef floorDef = TerrainDefOf.Concrete;
            //resolveParams.floorDef = floorDef;
            BaseGen.symbolStack.Push("edgeStreet", resolveParams);
            ResolveParams resolveParams2 = rp;
            resolveParams2.rect = rp.rect.ContractedBy(1);
            BaseGen.symbolStack.Push("zombieBasePart_outdoors_leaf", resolveParams2);
        }
    }
}