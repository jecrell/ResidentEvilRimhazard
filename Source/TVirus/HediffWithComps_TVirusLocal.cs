using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class HediffWithComps_TVirusLocal : Hediff_Injury
    {
        public override bool ShouldRemove => base.ShouldRemove || PawnIsImmune;


        private bool? pawnIsImmune = null;
        public bool PawnIsImmune
        {
            get
            {
                if (pawnIsImmune == null)
                {
                    pawnIsImmune = false;
                    if (this.pawn != null && this.pawn.story != null &&
                        this.pawn.story.traits != null &&
                        this.pawn.story.traits.HasTrait(TraitDef.Named("RE_TVirusImmunity")))
                        pawnIsImmune = true;
                }
                return pawnIsImmune.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Keeps track of the actual T-Virus controller that
        ///   controls the local sites of infection.
        /// </summary>
        private HediffWithComps_TVirus tvirusGlobalComp = null;
        public HediffWithComps_TVirus TVirusGlobalComp
        {
            get
            {
                if (tvirusGlobalComp == null)
                {
                    var getAttempt = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RE_TVirus"));
                    if (getAttempt == null)
                    {
                        //HediffGiverUtility.TryApply(pawn, HediffDef.Named("RE_TVirus"), new List<BodyPartDef> { BodyPartDefOf.Body });
                        HealthUtility.AdjustSeverity(pawn, HediffDef.Named("RE_TVirus"), 0.1f);
                        tvirusGlobalComp = (HediffWithComps_TVirus)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RE_TVirus"));
                    }
                    else
                        tvirusGlobalComp = (HediffWithComps_TVirus)getAttempt;
                }
                return tvirusGlobalComp;
            }
        }

        /// <summary>
        /// Makes a quick check that also adds the main T-Virus
        /// </summary>
        /// <param name="dinfo"></param>
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (TVirusGlobalComp == null)
                Log.Warning($"Unable to add T-Virus to {pawn.Label}");
        }



        /// <summary>
        /// After a certain period of time, the local infection sites
        ///   have an opportunity to spread to other connected body parts.
        /// </summary>
        public void Notify_SpreadToNextUninfectedPart()
        {
            List<BodyPartRecord> connectedParts = new List<BodyPartRecord>(GetInfectableParts());

            BodyPartRecord partRecord = connectedParts.RandomElement();
            Hediff hediff2 = HediffMaker.MakeHediff(HediffDef.Named("RE_TVirusLocalHediff"), pawn, partRecord);
            pawn.health.AddHediff(hediff2);
        }


        /// <summary>
        /// Finds connected parts that are yet to be infected by the T-Virus.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BodyPartRecord> GetInfectableParts()
        {
            List<BodyPartRecord> connectedParts = new List<BodyPartRecord>();

            connectedParts.AddRange(this.Part.parts);

            if (this.Part.parent != null)
            {
                connectedParts.Add(this.Part.parent);
                connectedParts.AddRange(this.Part.parent.parts);
            }

            //---------------------
            //-- Infection Rules --
            //---------------------
            //1. Do not infect the body.
            //2. Do not infect untouchable parts.
            //3. Do not infect missing parts.
            //4. Do not infect already infected parts.
            connectedParts.RemoveAll(
                x => 
                    x.def == BodyPartDefOf.Body ||
                    x.coverageAbs <= 0f ||
                    this.pawn.health.hediffSet.hediffs.Any
                    (
                        y => 
                        (
                            y.def.defName == "RE_TVirusLocal" ||
                            y is Hediff_MissingPart
                        )
                        && y.Part == x
                    )
            );
            return connectedParts;
        }


    }
}
