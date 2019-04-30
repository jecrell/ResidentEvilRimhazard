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
    class SymbolResolver_Interior_ZombieEncounter : SymbolResolver
{

        private static readonly IntRange ZombieCountRange = new IntRange(1, 3);

        public override void Resolve(ResolveParams rp)
        {
            var kindStr = "RE_ZombieKind";
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
            var count = ZombieCountRange.RandomInRange;
            for (int i = 0; i < count; i++)
            {
                var monsterParams = rp;
                monsterParams.singlePawnKindDef = PawnKindDef.Named(kindStr);
                BaseGen.symbolStack.Push("pawn", rp);
            }
        }
    }
}
