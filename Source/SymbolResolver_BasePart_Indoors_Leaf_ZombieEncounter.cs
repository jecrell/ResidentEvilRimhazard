using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RERimhazard
{
    class SymbolResolver_BasePart_Indoors_Leaf_ZombieEncounter : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            if (!base.CanResolve(rp))
            {
                return false;
            }
            if (BaseGen.globalSettings.basePart_barracksResolved < BaseGen.globalSettings.minBarracks)
            {
                return false;
            }
            return true;
        }

        public override void Resolve(ResolveParams rp)
        {
            BaseGen.symbolStack.Push("reZombieEncounter", rp);
        }
    }

}
