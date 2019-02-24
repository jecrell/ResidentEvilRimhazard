using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    public static class REUtility
    {
        /// <summary>
        /// Heals a character a certain added percentage
        /// </summary>
        /// <param name="injuredPawn">The character to be healed</param>
        /// <param name="percentage">The additional percentage to heal, for example: 30%. If the current injuries are about 40%, the total health percentage will be boosted until 70%.</param>
        public static void HealAddedPercentage(Pawn injuredPawn, float percentage)
        {
            //Added healing is capped at 100%, and if the percentage is lower, exit
            percentage = Mathf.Clamp(percentage, 0, 1.0f);
            if (percentage <= 0) return;

            //Our character must exist and have calculatable health
            if (injuredPawn == null) return;
            if (!injuredPawn.Spawned) return;
            if (injuredPawn.health == null) return;
            if (injuredPawn.health.summaryHealth == null) return;

            
            //Find a new maximum healing percentage
            float maximumHealingPct = Mathf.Clamp(injuredPawn.health.summaryHealth.SummaryHealthPercent + percentage, 0f, 1.0f);
            
            //Heal each injury until the healing percentage is at or above the maximum healing percentage
            foreach (var injury in injuredPawn.health.hediffSet.GetInjuriesTendable())
            {
                if (injuredPawn.health.summaryHealth.SummaryHealthPercent >= maximumHealingPct)
                    break;
                injury.Heal(99999f);
            }

        }
        
    }
}
