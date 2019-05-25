using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    class SymbolResolver_ZombieBasePart_Outdoors_LeafPossiblyDecorated : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (rp.rect.Width >= 10 && rp.rect.Height >= 10 && Rand.Chance(0.25f))
            {
                BaseGen.symbolStack.Push("zombieBasePart_outdoors_leafDecorated", rp);
            }
            else
            {
                BaseGen.symbolStack.Push("zombieBasePart_outdoors_leaf", rp);
            }
        }
    }
}