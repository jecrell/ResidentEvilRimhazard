using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class HediffWithComps_TVirus : HediffWithComps
    {
        /// <summary>
        /// A lazy value to keep track of the number of sites.
        /// When the number of sites becomes less than 1,
        ///  for instance, in the event of an amputation,
        ///  that is when we should remove the T-virus.
        /// </summary>
        private int? numOfSites = null;
        public int NumOfSites
        {
            get
            {
                if (numOfSites == null)
                {
                    int newValue = 0;
                    foreach (var site in pawn.health.hediffSet.GetHediffs<HediffWithComps_TVirusLocal>())
                    {
                        newValue++;
                    }
                    numOfSites = newValue;
                }
                return numOfSites.Value;
            }
        }
        public override bool ShouldRemove 
            => base.ShouldRemove 
            || NumOfSites <= 0;


        /// <summary>
        /// The T-Virus will continue to add new infection
        /// sites throughout the body as the character continues
        /// to live.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            if (GenTicks.TicksGame % 1500 == 0)
            {
                if (pawn.Spawned && !pawn.Dead)
                {
                    var site = pawn.health.hediffSet.GetHediffs<HediffWithComps_TVirusLocal>().RandomElement();
                    site.Notify_SpreadToNextUninfectedPart();
                    numOfSites = null;
                    this.Severity = 0.1f * NumOfSites;
                }

                if (this.Severity > 0.9f)
                {
                    CreateZombie();
                }
            }
        }

        /// <summary>
        /// After the virus spreads greatly throughout the body,
        ///   the human will die and a zombie will be reborn.
        /// </summary>
        public void CreateZombie()
        {
            Pawn thisPawn = this.pawn;
            this.pawn.Kill(null, this);
            //TODO Zombie creation
        }

    }
}
