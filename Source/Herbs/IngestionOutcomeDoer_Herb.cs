using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class IngestionOutcomeDoer_Herb : IngestionOutcomeDoer
    {
        
        /// <summary>
        /// Herbs in the Resident Evil franchise provide healing to characters in need of it.
        /// They also provide other effects, and the following calculation is for immediate
        /// benefits provided from ingestion.
        /// 
        /// TODO Add other effects rather than just healing.
        /// 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="ingested"></param>
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            var nameOfMixedHerbThingdef = ingested.def.defName;


            //Determine number of "greens" mixed in
            var numOfGreenHerbsMixedIn = FindNumberOfSubstrings(nameOfMixedHerbThingdef, "Green");

            //Determine number of "reds" mixed in
            var redHerbFactor = 1 + FindNumberOfSubstrings(nameOfMixedHerbThingdef, "Red");

            //Determine number of "blues" mixed in
            var blueHerbFactor = 1 + FindNumberOfSubstrings(nameOfMixedHerbThingdef, "Blue");

            switch (numOfGreenHerbsMixedIn)
            {
                case 0:
                    return;
                case 1:
                    REUtility.HealAddedPercentage(pawn, 0.15f * (redHerbFactor + blueHerbFactor));
                    break;
                case 2:
                    REUtility.HealAddedPercentage(pawn, 0.3f * (redHerbFactor + blueHerbFactor));
                    break;
                case 3:
                    REUtility.HealAddedPercentage(pawn, 0.9f * (redHerbFactor + blueHerbFactor));
                    break;
                default:
                    REUtility.HealAddedPercentage(pawn, 0.15f * (redHerbFactor + blueHerbFactor));
                    break;
            }
        }

        private static int FindNumberOfSubstrings(string mainString, string subString)
        {
            int numOfSubstrings = 0;
            while (mainString.Contains(subString))
            {
                mainString.ReplaceFirst(subString, "");
                numOfSubstrings++;
            }
            return numOfSubstrings;
        }
    }
}
