using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    class SymbolResolver_Interior_CrimsonHeadEncounter : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var kindStr = "RE_CrimsonHeadKind";
            BaseGen.symbolStack.Push("indoorLighting", rp);
            BaseGen.symbolStack.Push("randomlyPlaceMealsOnTables", rp);
            BaseGen.symbolStack.Push("placeChairsNearTables", rp);
            int num = Mathf.Max(GenMath.RoundRandom((float)rp.rect.Area / 20f), 1);
            for (int i = 0; i < num; i++)
            {
                ResolveParams resolveParams = rp;
                resolveParams.singleThingDef = ThingDefOf.Table2x2c;
                BaseGen.symbolStack.Push("thing", resolveParams);
            }
            var monsterParams = rp;
            monsterParams.singlePawnKindDef = PawnKindDef.Named(kindStr);
            BaseGen.symbolStack.Push("pawn", rp);
        }
    }
}
