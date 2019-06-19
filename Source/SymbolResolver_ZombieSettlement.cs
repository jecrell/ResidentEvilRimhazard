// RimWorld.BaseGen.SymbolResolver_Settlement
using RimWorld;
using RimWorld.BaseGen;
using System;
using Verse;
using Verse.AI.Group;

namespace RERimhazard
{

    public class SymbolResolver_ZombieSettlement : SymbolResolver
    {
        public static readonly FloatRange DefaultPawnsPoints = new FloatRange(2250f, 2500f);

        public override void Resolve(ResolveParams rp)
        {
            REDataCache.TyrantEncounterGenerated = false;
            REDataCache.ZombieDogEncounterGenerated = false;
            REDataCache.GEncounterGenerated = false;

            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            rp.wallStuff = ThingDefOf.Steel;
            rp.floorDef = TerrainDef.Named("BrokenAsphalt");
            int num = 0;
            int? edgeDefenseWidth = rp.edgeDefenseWidth;
            if (edgeDefenseWidth.HasValue)
            {
                num = rp.edgeDefenseWidth.Value;
            }
            else if (rp.rect.Width >= 20 && rp.rect.Height >= 20 && ((int)faction.def.techLevel >= 4 || Rand.Bool))
            {
                num = ((!Rand.Bool) ? 4 : 2);
            }
            float num2 = (float)rp.rect.Area / 144f * 0.17f;
            BaseGen.globalSettings.minEmptyNodes = ((!(num2 < 1f)) ? GenMath.RoundRandom(num2) : 0);
            Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendZombieBase(faction, map.Center), map);
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);

            
            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
            resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms))));
            if (resolveParams.pawnGroupMakerParams == null)
            {
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams.pawnGroupMakerParams.tile = map.Tile;
                resolveParams.pawnGroupMakerParams.faction = faction;
                PawnGroupMakerParms pawnGroupMakerParams = resolveParams.pawnGroupMakerParams;
                float? settlementPawnGroupPoints = rp.settlementPawnGroupPoints;
                pawnGroupMakerParams.points = ((!settlementPawnGroupPoints.HasValue) ? DefaultPawnsPoints.RandomInRange : settlementPawnGroupPoints.Value);
                resolveParams.pawnGroupMakerParams.inhabitants = true;
                resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }
            BaseGen.symbolStack.Push("pawnGroup", resolveParams);
            BaseGen.symbolStack.Push("outdoorLighting", rp);
            ResolveParams resolveParams4 = rp;
            resolveParams4.rect = rp.rect.ContractedBy(num);
            resolveParams4.faction = faction;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4);

            if (Find.Scenario.name == "Resident Evil - Umbrella Corp")
            {
                ResolveParams umbrellaCorp = rp;
                umbrellaCorp.rect = new CellRect(map.Center.x - 9, map.Center.z - 9, 18, 18);
                umbrellaCorp.wallStuff = ThingDefOf.Plasteel;
                umbrellaCorp.floorDef = TerrainDef.Named("SterileTile");
                umbrellaCorp.faction = Faction.OfPlayer;
                BaseGen.symbolStack.Push("emptyRoom", umbrellaCorp);
                BaseGen.symbolStack.Push("floor", umbrellaCorp);
                BaseGen.symbolStack.Push("clear", umbrellaCorp);
            }

            ResolveParams resolveParams5 = rp;
            resolveParams5.rect = rp.rect.ContractedBy(num);
            resolveParams5.faction = faction;
            BaseGen.symbolStack.Push("zombieBasePart_outdoors", resolveParams5);
            ResolveParams resolveParams6 = rp;

            //resolveParams6.rect = rp.rect.ExpandedBy(3);
            //resolveParams6.floorDef = TerrainDef.Named("BrokenAsphalt");
            //bool? floorOnlyIfTerrainSupports = rp.floorOnlyIfTerrainSupports;
            //resolveParams6.floorOnlyIfTerrainSupports = false;
            //BaseGen.symbolStack.Push("floor", resolveParams6);
            //Clear the land...
            ResolveParams clearParams = rp;
            clearParams.rect = rp.rect;
            BaseGen.symbolStack.Push("clear", clearParams);

        }
    }
}