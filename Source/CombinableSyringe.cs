using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public class CombinableSyringe : Combinable
    {
        private ThingDef result = null;

        public override ThingDef GetCombineResult(string one, string two)
        {
            return null;
        }

        public override bool CanCombine(Thing a, Thing b)
        {
            if (b is Pawn)
                return true;
            return false;
        }

        public override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters() { canTargetPawns = true, canTargetSelf = false, canTargetBuildings = false, canTargetItems = false };
        }

        public void InjectionEffect(Pawn target)
        {
            var hediffString = this.def.defName.Replace("RE_Syringe", "");
            Log.Message(hediffString);
            target.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("RE_TVirusScratch"), 3, 1.0f, -1, null, target.health.hediffSet.GetNotMissingParts().Where(x => x.def.alive).RandomElement()));
            //HealthUtility.AdjustSeverity(target, HediffDef.Named(hediffString), 0.1f);
            //HediffGiverUtility.TryApply(target, HediffDef.Named(hediffString), new List<BodyPartDef> { BodyPartDefOf.Body });
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
                            Job job = new Job(DefDatabase<JobDef>.GetNamed("RE_Inject"), b,a);
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
