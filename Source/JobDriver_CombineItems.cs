using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public class JobDriver_CombineItems : JobDriver
    {
        private const TargetIndex ItemAIndex = TargetIndex.A;
        private const TargetIndex ItemBIndex = TargetIndex.B;
        private string customString = "";

        protected Thing Thing => (Thing)base.job.GetTarget(TargetIndex.A).Thing;

        protected Thing BrainChip => (Thing)base.job.GetTarget(TargetIndex.B).Thing;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);

            yield return Toils_Reserve.Reserve(ItemAIndex, 1);
            yield return Toils_Reserve.Reserve(ItemBIndex, 1);

            yield return Toils_Goto.GotoThing(ItemBIndex, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(ItemBIndex);
            yield return Toils_Construct.UninstallIfMinifiable(ItemBIndex).FailOnSomeonePhysicallyInteracting(ItemBIndex);
            yield return Toils_Haul.StartCarryThing(ItemBIndex, false, true);
            yield return Toils_Goto.GotoThing(ItemAIndex, PathEndMode.Touch);
            Toil chantingTime = new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 1200
            };
            chantingTime.WithProgressBarToilDelay(ItemAIndex, false, -0.5f);
            yield return chantingTime;
            yield return new Toil
            {
                initAction = delegate
                {
                    IntVec3 position = this.Thing.Position;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Near, out Thing thingB, null);
                    CombineItems(thingB, Thing);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return Toils_Reserve.Release(TargetIndex.B);

            //Toil 9: Think about that.
            yield return new Toil
            {
                initAction = delegate
                {
                    ////It's a day to remember
                    //TaleDef taleToAdd = TaleDef.Named("HeldSermon");
                    //if ((this.pawn.IsColonist || this.pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
                    //{
                    //    TaleRecorder.RecordTale(taleToAdd, new object[]
                    //    {
                    //       this.pawn,
                    //    });
                    //}
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield break;


        }

        public override string GetReport()
        {
            if (this.customString == "")
            {
                return base.GetReport();
            }
            return this.customString;
        }


        private void CombineItems(Thing thingB, Thing thingA)
        {
            Combinable c = thingA as Combinable;
            ThingDef result = c.GetCombineResult(thingA.def.defName, thingB.def.defName);
            var newThing = ThingMaker.MakeThing(result);
            GenPlace.TryPlaceThing(newThing, GetActor().Position, GetActor().Map, ThingPlaceMode.Near);
            if (thingA.stackCount > 1)
                thingA.stackCount -= 1;
            else
                thingA.Destroy();
            thingB.Destroy();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
