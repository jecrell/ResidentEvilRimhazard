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
    public class JobDriver_HaulZombie : JobDriver
    {
        private const TargetIndex TakeeIndex = TargetIndex.A;
        private const TargetIndex DestinationIndex = TargetIndex.B;
        private string customString = "";

        protected PawnRelocatable Takee => (PawnRelocatable)base.job.GetTarget(TargetIndex.A).Thing;

        protected IntVec3 DropLocation => base.job.GetTarget(TargetIndex.B).Cell;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_Reserve.Reserve(TakeeIndex, 1);
            yield return Toils_Reserve.Reserve(DestinationIndex, 1);

            yield return new Toil
            {
                initAction = delegate
                {
                    this.customString = "Pawn_GatheringPawnForDrop".Translate();
                }
            };

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    Takee.isMoving = true;
                }
            };
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.ClosestTouch);
            Toil droppingTime = new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 100
            };
            droppingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            droppingTime.initAction = delegate
            {
                this.customString = "Pawn_PawnDropping".Translate(this.Takee.LabelShort
                    );
            };
            yield return droppingTime;
            yield return new Toil
            {
                initAction = delegate
                {
                    this.customString = "Pawn_PawnDropFinished".Translate();
                    IntVec3 position = this.DropLocation;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out Thing thing, null);

                    SacrificeCompleted();
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


        private void SacrificeCompleted()
        {
            this.Takee.isMoving = false;
            this.Takee.Position = this.DropLocation;
            this.Takee.Notify_Teleported(false);
            this.Takee.stances.CancelBusyStanceHard();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}