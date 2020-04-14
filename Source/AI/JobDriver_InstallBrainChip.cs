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
    public class JobDriver_InstallBrainChip : JobDriver
    {
        private const TargetIndex ZombieIndex = TargetIndex.A;
        private const TargetIndex BrainChipIndex = TargetIndex.B;
        private string customString = "";

        protected Zombie Zombie => (Zombie)base.job.GetTarget(TargetIndex.A).Thing;

        protected Thing BrainChip => (Thing)base.job.GetTarget(TargetIndex.B).Thing;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);

            yield return Toils_Reserve.Reserve(ZombieIndex, 1);
            yield return Toils_Reserve.Reserve(BrainChipIndex, 1);

            yield return new Toil
            {
                initAction = delegate
                {
                    REDataCache.ClearBrainChipCache(this.GetActor().Map);
                    //this.customString = "ChthonianPitSacrificeGathering".Translate();
                }
            };

            yield return Toils_Goto.GotoThing(BrainChipIndex, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(BrainChipIndex);
            yield return Toils_Construct.UninstallIfMinifiable(BrainChipIndex).FailOnSomeonePhysicallyInteracting(BrainChipIndex);
            yield return Toils_Haul.StartCarryThing(BrainChipIndex, false, true);
            yield return Toils_Goto.GotoThing(ZombieIndex, PathEndMode.Touch);
            Toil chantingTime = new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 1200
            };
            chantingTime.WithProgressBarToilDelay(ZombieIndex, false, -0.5f);
            chantingTime.initAction = delegate
            {
                //this.customString = "ChthonianPitSacrificeDropping".Translate(new object[]
                //    {
                //        this.Zombie.LabelShort
                //    });
            };
            yield return chantingTime;
            yield return new Toil
            {
                initAction = delegate
                {
                    //this.customString = "ChthonianPitSacrificeFinished".Translate();
                    IntVec3 position = this.Zombie.Position;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Near, out Thing thing, null);
                    InstallBrainChip(thing, Zombie);
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


        private void InstallBrainChip(Thing brainChip, Zombie zombie)
        {
            if (!brainChip.DestroyedOrNull())
                brainChip.Destroy();
            zombie.installedBrainChip = true;
            zombie.guest.SetGuestStatus(Faction.OfPlayer, true);
            Messages.Message("RE_BrainChipInstalled".Translate(zombie.KindLabel), zombie, MessageTypeDefOf.PositiveEvent);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
