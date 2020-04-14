// RimWorld.IncidentWorker_RefugeeChased
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RERimhazard
{

    public class IncidentWorker_RefugeeChasedZombies : IncidentWorker
    {
        private static readonly IntRange RaidDelay = new IntRange(1000, 4000);

        private static readonly FloatRange RaidPointsFactorRange = new FloatRange(1f, 1.6f);

        private const float RelationWithColonistWeight = 20f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            IntVec3 spawnSpot;
            Faction enemyFac;
            return TryFindSpawnSpot(map, out spawnSpot) && TryFindEnemyFaction(out enemyFac);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!TryFindSpawnSpot(map, out IntVec3 spawnSpot))
            {
                return false;
            }
            if (!TryFindEnemyFaction(out Faction enemyFac))
            {
                return false;
            }
            int @int = Rand.Int;
            IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            raidParms.forced = true;
            raidParms.faction = enemyFac;
            raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            raidParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            raidParms.spawnCenter = spawnSpot;
            raidParms.points = Mathf.Max(raidParms.points * RaidPointsFactorRange.RandomInRange, enemyFac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            raidParms.pawnGroupMakerSeed = @int;
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms);
            defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, raidParms.raidArrivalMode, raidParms.raidStrategy, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
            IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, colonistRelationChanceFactor: 20f);
            Pawn refugee = PawnGenerator.GeneratePawn(request);
            refugee.relations.everSeenByPlayer = true;
            string text = "RefugeeChasedInitial".Translate(refugee.Name.ToStringFull, refugee.story.Title, enemyFac.def.pawnsPlural, enemyFac.Name, refugee.ageTracker.AgeBiologicalYears, PawnUtility.PawnKindsToCommaList(pawnKinds, useAnd: true), refugee.Named("PAWN"));
            text = text.AdjustedFor(refugee);
            TaggedString temp = new TaggedString(text);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref temp, refugee);
            DiaNode diaNode = new DiaNode(temp);
            DiaOption diaOption = new DiaOption("RefugeeChasedInitial_Accept".Translate());
            diaOption.action = delegate
            {
                GenSpawn.Spawn(refugee, spawnSpot, map);
                refugee.SetFaction(Faction.OfPlayer);
                CameraJumper.TryJump(refugee);
                QueuedIncident qi = new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, raidParms), Find.TickManager.TicksGame + RaidDelay.RandomInRange);
                Find.Storyteller.incidentQueue.Add(qi);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            string text2 = "RefugeeChasedRejected".Translate(refugee.LabelShort, refugee);
            DiaNode diaNode2 = new DiaNode(text2);
            DiaOption diaOption2 = new DiaOption("OK".Translate());
            diaOption2.resolveTree = true;
            diaNode2.options.Add(diaOption2);
            DiaOption diaOption3 = new DiaOption("RefugeeChasedInitial_Reject".Translate());
            diaOption3.action = delegate
            {
                Find.WorldPawns.PassToWorld(refugee);
            };
            diaOption3.link = diaNode2;
            diaNode.options.Add(diaOption3);
            string title = "RefugeeChasedTitle".Translate(map.Parent.Label);
            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, enemyFac, delayInteractivity: true, radioMode: true, title: title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title, enemyFac));
            return true;
        }

        private bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            Predicate<IntVec3> validator = (IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        private bool TryFindEnemyFaction(out Faction enemyFac)
        {
            enemyFac = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("RE_Zombies"));
            return true;
        }
    }

}
