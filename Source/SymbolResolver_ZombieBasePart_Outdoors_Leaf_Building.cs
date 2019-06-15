using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RERimhazard
{
    public class SymbolResolver_ZombieBasePart_Outdoors_Leaf_Building : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            if (!base.CanResolve(rp))
            {
                return false;
            }
            if (BaseGen.globalSettings.basePart_emptyNodesResolved < BaseGen.globalSettings.minEmptyNodes && BaseGen.globalSettings.basePart_buildingsResolved >= BaseGen.globalSettings.minBuildings)
            {
                return false;
            }
            return true;
        }

        public override void Resolve(ResolveParams rp)
        {
            ResolveParams resolveParams = rp;
            //resolveParams.wallStuff = (rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction));
            //resolveParams.floorDef = (rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, allowCarpet: true));
            BaseGen.symbolStack.Push("zombieBasePart_indoors", resolveParams);
            BaseGen.globalSettings.basePart_buildingsResolved++;
        }
    }
}
