using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public struct InputSet
    {
        public string a;
        public string b;

        public InputSet(string newA, string newB)
        {
            a = newA;
            b = newB;
        }
    }

    public class CombinableHerb : Combinable
    {
        private ThingDef result = null;

        public override ThingDef GetCombineResult(string one, string two)
        {
            one = one.Replace("RE_Medicine", "");
            two = two.Replace("RE_Medicine", "");
            Log.Message("RE_Medicine" + one + two);
            //if (DefDatabase<ThingDef>.GetNamedSilentFail("RE_Medicine" + one + two) is ThingDef resultOne)
            //{
            //    return resultOne;
            //}
            //else if (DefDatabase<ThingDef>.GetNamedSilentFail("RE_Medicine" + two + one) is ThingDef resultTwo)
            //{
            //    return resultTwo;
            //}
            //else
            //{
                int numOfGreens = IngestionOutcomeDoer_Herb.FindNumberOfSubstrings(one + two, "Green");
                int numOfReds = IngestionOutcomeDoer_Herb.FindNumberOfSubstrings(one + two, "Red");
                int numOfBlues = IngestionOutcomeDoer_Herb.FindNumberOfSubstrings(one + two, "Blue");
                if (numOfBlues == 0 && numOfGreens == 0 && numOfReds == 0)
                    return null;

                //Green first
                string testResult = "RE_Medicine";
                for (int i = 0; i < numOfGreens; i++)
                    testResult = testResult + "Green";
                for (int j = 0; j < numOfReds; j++)
                    testResult = testResult + "Red";
                for (int k = 0; k < numOfBlues; k++)
                    testResult = testResult + "Blue";
                Log.Message(testResult);
                if (DefDatabase<ThingDef>.GetNamedSilentFail(testResult) is ThingDef testResultAlpha)
                {
                    return testResultAlpha;
                }


                //Red first
                testResult = "RE_Medicine";
                for (int q = 0; q < numOfReds; q++)
                    testResult = testResult + "Red";
                for (int w = 0; w < numOfBlues; w++)
                    testResult = testResult + "Blue";
                for (int e = 0; e < numOfGreens; e++)
                    testResult = testResult + "Green";
                Log.Message(testResult);
                if (DefDatabase<ThingDef>.GetNamedSilentFail(testResult) is ThingDef testResultBeta)
                {
                    return testResultBeta;
                }

                //Blue first
                testResult = "RE_Medicine";
                for (int a = 0; a < numOfBlues; a++)
                    testResult = testResult + "Blue";
                for (int s = 0; s < numOfGreens; s++)
                    testResult = testResult + "Green";
                for (int d = 0; d < numOfReds; d++)
                    testResult = testResult + "Red";
                Log.Message(testResult);
                if (DefDatabase<ThingDef>.GetNamedSilentFail(testResult) is ThingDef testResultGamma)
                {
                    return testResultGamma;
                }
            //}
            return null;
        }

        public override bool CanCombine(Thing a, Thing b)
        {
            string one = a.def.defName;
            string two = b.def.defName;
         
            if (GetCombineResult(one, two) is ThingDef mixedHerb)
            {
                result = mixedHerb;
                return true;
            }
            result = null;
            return false;
        }

        public override void CombineAction(Thing a, Thing b)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            Map map = this.Map;
            var colonists = map.mapPawns.FreeColonistsSpawned;
            if (colonists.Count() != 0)
            {
                foreach (Pawn current in colonists)
                {
                    if (!current.Dead)
                    {
                        string text = current.Name.ToStringFull;
                        List<FloatMenuOption> arg_121_0 = list;
                        Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, current);
                        arg_121_0.Add(new FloatMenuOption(text, delegate
                        {
                            Job job = new Job(DefDatabase<JobDef>.GetNamed("RE_Combine"), a, b);
                            job.count = 1;
                            current.jobs.TryTakeOrderedJob(job);
                        }, MenuOptionPriority.Default, null, null, 29f, extraPartOnGUI, null));
                    }
                }
            }
            else
            {
                list.Add(new FloatMenuOption("NoColonists".Translate(), delegate
                {
                }, MenuOptionPriority.Default));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}
