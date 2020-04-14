using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public class JobDriver_EatOffFloor : JobDriver
    {
        private bool usingNutrientPasteDispenser;

        private bool eatingFromInventory;

        public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

        public const TargetIndex IngestibleSourceInd = TargetIndex.A;

        private const TargetIndex TableCellInd = TargetIndex.B;

        private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

        private Thing IngestibleSource => job.GetTarget(TargetIndex.A).Thing;

        private float ChewDurationMultiplier
        {
            get
            {
                Thing ingestibleSource = IngestibleSource;
                if (ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat)
                {
                    return 1f * 15f;
                }
                return (1f / pawn.GetStatValue(StatDefOf.EatingSpeed)) * 15f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref usingNutrientPasteDispenser, "usingNutrientPasteDispenser", defaultValue: false);
            Scribe_Values.Look(ref eatingFromInventory, "eatingFromInventory", defaultValue: false);
        }

        public override string GetReport()
        {
            if (usingNutrientPasteDispenser)
            {
                return job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label);
            }
            Thing thing = job.targetA.Thing;
            if (thing != null && thing.def.ingestible != null)
            {
                if (!thing.def.ingestible.ingestReportStringEat.NullOrEmpty() && (thing.def.ingestible.ingestReportString.NullOrEmpty() || (int)pawn.RaceProps.intelligence < 1))
                {
                    return string.Format(thing.def.ingestible.ingestReportStringEat, job.targetA.Thing.LabelShort);
                }
                if (!thing.def.ingestible.ingestReportString.NullOrEmpty())
                {
                    return string.Format(thing.def.ingestible.ingestReportString, job.targetA.Thing.LabelShort);
                }
            }
            return base.GetReport();
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            usingNutrientPasteDispenser = (IngestibleSource is Building_NutrientPasteDispenser);
            eatingFromInventory = (pawn.inventory != null && pawn.inventory.Contains(IngestibleSource));
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (!usingNutrientPasteDispenser)
            {
                this.FailOn(() => !IngestibleSource.Destroyed && !IngestibleSource.IngestibleNow);
            }
            Toil chew = Toils_Ingest.ChewIngestible(pawn, ChewDurationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => (!IngestibleSource.Spawned && (pawn.carryTracker != null)) || pawn.mindState.anyCloseHostilesRecently).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            foreach (Toil item in PrepareToIngestToils(chew))
            {
                yield return item;
            }
            yield return chew;
            yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.A);
            yield return Toils_Jump.JumpIf(chew, () => job.GetTarget(TargetIndex.A).Thing is Corpse && pawn.needs.food.CurLevelPercentage < 0.9f);
        }

        private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        }
    }

}
