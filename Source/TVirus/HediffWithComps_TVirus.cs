﻿using RimWorld;
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
            || NumOfSites <= 0 || PawnIsImmune;

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
        /// A simple variable keeping track of how long between
        ///   the spreading of infection sites for the T-Virus.
        /// This variable will be overridden by RESettings after
        ///   its first call.
        /// </summary>
        private int nextSpreadTicks = 1500;


        public override void Tick()
        {
            base.Tick();

            TVirusTick();
        }

        /// <summary>
        /// The T-Virus will continue to add new infection
        /// sites throughout the body as the character continues
        /// to live.
        /// </summary>
        private void TVirusTick()
        {
            try
            {

                if (GenTicks.TicksGame % nextSpreadTicks == 0)
                {
                    nextSpreadTicks = RESettings.SPREADTIME.RandomInRange;
                    if (pawn.Spawned && !pawn.Dead)
                    {
                        var hediffs = pawn?.health?.hediffSet?.GetHediffs<HediffWithComps_TVirusLocal>();
                        if (hediffs != null)
                        {
                            var sites =
                                from s in hediffs
                                where
                                    s?.GetInfectableParts()?.Count() > 0
                                select s;

                            if (sites != null)
                            {
                                var site = sites?.InRandomOrder().FirstOrDefault();
                                if (site != null)
                                {
                                    site.Notify_SpreadToNextUninfectedPart();
                                    numOfSites = null;
                                    this.Severity = 0.1f * NumOfSites;
                                }
                            }
                        }
                    }

                    if (this.Severity > 0.95f)
                    {
                        CreateZombie();
                    }
                }
            }
            catch
            {
                
            }
        }

        /// <summary>
        /// After the virus spreads greatly throughout the body,
        ///   the human will die and a zombie will be reborn.
        /// </summary>
        public void CreateZombie()
        {
            Pawn thisPawn = this.pawn;
            thisPawn.MapHeld.GetComponent<MapComponent_ZombieTracker>().AddInfectedDeadLocation(thisPawn);
            this.pawn.Kill(null, this);
        }

    }
}
