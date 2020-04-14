using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public class Building_StairsDown : Building
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
            {
                yield return opt;
            }

            if (Find.World.GetComponent<WorldComponent_ZLevels>().HasZLevelsBelow(Tile, Map.Parent))
            {
                yield return new FloatMenuOption("Go down", () =>
                {
                    Job job = new Job(DefDatabase<JobDef>.GetNamed("RE_GoToStairs"), this);
                    selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                });
            }
        }
    }
}