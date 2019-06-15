using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RERimhazard
{
    class SymbolResolver_Interior_LickerEncounter : SymbolResolver
{
        public override void Resolve(ResolveParams rp)
        {
            var kindStr = "RE_LickerKind";
            BaseGen.symbolStack.Push("indoorLighting", rp);
            //BaseGen.symbolStack.Push("randomlyPlaceMealsOnTables", rp);
            //BaseGen.symbolStack.Push("placeChairsNearTables", rp);
            //int num = Mathf.Max(GenMath.RoundRandom((float)rp.rect.Area / 20f), 1);
            //for (int i = 0; i < num; i++)
            //{
            //    ResolveParams resolveParams = rp;
            //    resolveParams.singleThingDef = ThingDefOf.Table2x2c;
            //    BaseGen.symbolStack.Push("thing", resolveParams);
            //}
            var monsterParams = rp;
            monsterParams.pawnGroupMakerParams = null;
            monsterParams.rect = rp.rect;
            monsterParams.singlePawnKindDef = PawnKindDef.Named(kindStr);
            //Lord singlePawnLord = LordMaker.MakeNewLord(monsterParams.faction, new LordJob_DefendPoint(monsterParams.rect.CenterCell), BaseGen.globalSettings.map);
            //monsterParams.singlePawnLord = singlePawnLord;
            BaseGen.symbolStack.Push("pawn", monsterParams);
        }
    }
}
