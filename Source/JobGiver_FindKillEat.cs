using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public class JobGiver_FindKillEat : ThinkNode_JobGiver
    {
        private const float WaitChance = 0.75f;

        private const int WaitTicks = 90;

        private const int MinMeleeChaseTicks = 420;

        private const int MaxMeleeChaseTicks = 900;

        private const int WanderOutsideDoorRegions = 9;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null) == null)
            {
                return null;
            }
            if (!pawn.mindState.anyCloseHostilesRecently)
            {
                Corpse corpse2 = this.FindCorpseTarget(pawn);
                if (corpse2 != null && pawn.CanReach(corpse2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    return this.EatCorpseJob(pawn, corpse2);
                }
            }
            Pawn pawn2 = this.FindPawnTarget(pawn);
            if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                return this.MeleeAttackJob(pawn, pawn2);
            }

            Building building = this.FindTurretTarget(pawn);
            if (building != null)
            {
                return this.MeleeAttackJob(pawn, building);
            }
            if (pawn2 != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    IntVec3 cellBeforeBlocker;
                    Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                    if (thing != null)
                    {
                        //Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true);
                        //if (job != null)
                        //{
                        return this.MeleeAttackJob(pawn, thing);
                        //}
                    }
                    IntVec3 loc = pawnPath.LastCellBeforeBlockerOrFinalCell(pawn.MapHeld);
                    IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
                    if (randomCell == pawn.Position)
                    {
                        return new Job(JobDefOf.Wait, 30, false);
                    }
                    return new Job(JobDefOf.Goto, randomCell);
                }
            }
            Building buildingDoor = this.FindDoorTarget(pawn);
            if (buildingDoor != null)
            {
                return this.MeleeAttackJob(pawn, buildingDoor);
            }

            return null;
        }

        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            return new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(900, 1800),
                attackDoorIfTargetLost = true,
                locomotionUrgency = LocomotionUrgency.Sprint,
                killIncappedTarget = true
            };
        }


        private Job EatCorpseJob(Pawn pawn, Thing target)
        {
            return new Job(JobDefOf.Ingest, target)
            {
                expiryInterval = Rand.Range(900, 1800)
            };
        }



        private Corpse FindCorpseTarget(Pawn pawn)
        {
            return (Corpse)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable, (Thing x) => x is Corpse c && c?.InnerPawn?.def?.race?.intelligence >= Intelligence.ToolUser, 0f, 9999f, default(IntVec3), 3.40282347E+38f, true);
        }


        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachable, (Thing x) => x is Pawn p && !p.Dead && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f, default(IntVec3), 3.40282347E+38f, true);
        }


        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building, 0f, 70f, default(IntVec3), 3.40282347E+38f, false);
        }

        private Building_Door FindDoorTarget(Pawn pawn)
        {
            return (Building_Door)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing t) => t is Building_Door, 0f, 70f, default(IntVec3), 3.40282347E+38f, false);
        }
    }
}
