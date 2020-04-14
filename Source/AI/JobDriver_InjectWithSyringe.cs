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
    public class JobDriver_InjectWithSyringe : JobDriver
    {
        private const TargetIndex VictimIndex = TargetIndex.A;
        private const TargetIndex SyringeIndex = TargetIndex.B;

        protected Pawn Victim => (Pawn)base.job.GetTarget(TargetIndex.A).Thing;

        protected Thing Syringe => (Thing)base.job.GetTarget(TargetIndex.B).Thing;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);

            yield return Toils_Reserve.Reserve(VictimIndex, 1);
            yield return Toils_Reserve.Reserve(SyringeIndex, 1);

            yield return new Toil
            {
                initAction = delegate
                {
                    //REDataCache.ClearSyringeCache(this.GetActor().Map);
                    //this.customString = "ChthonianPitSacrificeGathering".Translate();
                }
            };

            yield return Toils_Goto.GotoThing(SyringeIndex, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(SyringeIndex);
            yield return Toils_Construct.UninstallIfMinifiable(SyringeIndex).FailOnSomeonePhysicallyInteracting(SyringeIndex);
            yield return Toils_Haul.StartCarryThing(SyringeIndex, false, true);
            yield return Toils_Goto.GotoThing(VictimIndex, PathEndMode.Touch);
            //Toil chantingTime = new Toil()
            //{
            //    defaultCompleteMode = ToilCompleteMode.Delay,
            //    defaultDuration = 1200
            //};
            //chantingTime.WithProgressBarToilDelay(VictimIndex, false, -0.5f);
            //chantingTime.initAction = delegate
            //{
            //this.customString = "ChthonianPitSacrificeDropping".Translate(new object[]
            //    {
            //        this.Victim.LabelShort
            //    });
            //};
            //yield return chantingTime;
            yield return Toils_General.WaitWith(VictimIndex, 500, true);
            yield return new Toil
            {
                initAction = delegate
                {
                    //this.customString = "ChthonianPitSacrificeFinished".Translate();
                    IntVec3 position = this.Victim.Position;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Near, out Thing thing, null);
                    InstallSyringe((CombinableSyringe)thing, Victim);
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

        private void InstallSyringe(CombinableSyringe Syringe, Pawn Victim)
        {
            Syringe.InjectionEffect(Victim);

            if (!Syringe.DestroyedOrNull())
                Syringe.Destroy();


            Messages.Message("RE_SyringeInjected".Translate(Victim.KindLabel), Victim, MessageTypeDefOf.PositiveEvent);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
