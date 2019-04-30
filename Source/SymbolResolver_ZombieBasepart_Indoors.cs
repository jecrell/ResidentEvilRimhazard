// RimWorld.BaseGen.SymbolResolver_BasePart_Indoors
using RimWorld.BaseGen;
using Verse;

namespace RERimhazard
{
    public class SymbolResolver_ZombieBasePart_Indoors : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (rp.rect.Width > 13 || rp.rect.Height > 13 || ((rp.rect.Width >= 9 || rp.rect.Height >= 9) && Rand.Chance(0.3f)))
            {
                BaseGen.symbolStack.Push("zombiebasePart_indoors_division", rp);
            }
            else
            {
                BaseGen.symbolStack.Push("zombiebasePart_indoors_leaf", rp);
            }
        }
    }

}
