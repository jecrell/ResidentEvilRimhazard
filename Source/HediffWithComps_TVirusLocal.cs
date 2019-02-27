using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class HediffWithComps_TVirusLocal : HediffWithComps
    {
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
                        HediffGiverUtility.TryApply(pawn, HediffDef.Named("RE_TVirus"), new List<BodyPartDef> { BodyPartDefOf.Body });
                        tvirusGlobalComp = (HediffWithComps_TVirus)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RE_TVirus"));
                    }
                    else
                        tvirusGlobalComp = (HediffWithComps_TVirus)getAttempt;
                }
                return tvirusGlobalComp;
            }
        }

        /// <summary>
        /// After a certain period of time, the local infection sites
        ///   have an opportunity to spread to other connected body parts.
        /// </summary>
        public void Notify_SpreadToNextUninfectedPart()
        {
            List<BodyPartRecord> connectedParts = new List<BodyPartRecord>();

            if (this.Part.GetDirectChildParts() is List<BodyPartRecord> childParts)
                connectedParts.AddRange(childParts);

            if (this.Part.parent != null)
                connectedParts.Add(this.Part.parent);

            connectedParts.RemoveAll(x =>
           this.pawn.health.hediffSet.hediffs.Any(y => y.def.defName == "RE_TVirusLocal" && y.Part == x));
            

            HediffGiverUtility.TryApply(pawn, HediffDef.Named("RE_TVirusLocal"), new List<BodyPartDef> { connectedParts?.RandomElement()?.def });

        }
    }
}
