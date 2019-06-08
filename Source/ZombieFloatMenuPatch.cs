using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JecsTools;
using Verse;
using UnityEngine;
using Verse.AI;
using RimWorld;

namespace RERimhazard
{
    public class ZombieFloatMenuPatch : FloatMenuPatch
    {
        public override IEnumerable<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>> GetFloatMenus()
        {
            List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>> floatMenus = new List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>();

            _Condition zombieCondition = new _Condition(_ConditionType.IsType, typeof(Zombie));
            Func<Vector3, Pawn, Thing, List<FloatMenuOption>> zombieFunc = delegate (Vector3 clickPos, Pawn pawn, Thing curThing)
            {
                List<FloatMenuOption> opts = null;
                if (curThing is Zombie target && target.Downed)
                {
                    if (REDataCache.GetFirstBrainChip(pawn.MapHeld) is Thing brainChip)
                    {
                        opts = new List<FloatMenuOption>();

                        if (target.installedBrainChip)
                        {
                            //Do nothing
                        }
                        else if (!pawn.CanReach(target, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            opts.Add(new FloatMenuOption("RE_CannotInstallBrainchip".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else if (!pawn.CanReserve(target, 1))
                        {
                            opts.Add(new FloatMenuOption("RE_CannotInstallBrainchip".Translate() + ": " + "Reserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else if (!pawn.CanReach(brainChip, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            opts.Add(new FloatMenuOption("RE_CannotInstallBrainchip".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else if (!pawn.CanReserve(brainChip, 1))
                        {
                            opts.Add(new FloatMenuOption("RE_CannotInstallBrainchip".Translate() + ": " + "Reserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else
                        {
                            Action action = delegate
                            {
                                Job job = new Job(DefDatabase<JobDef>.GetNamed("RE_InstallBrainChip"), target, brainChip);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            };
                            opts.Add(new FloatMenuOption("RE_InstallBrainChip".Translate(
                                    brainChip.LabelCap,
                                    target.LabelShortCap
                            ), action, MenuOptionPriority.High, null, target, 0f, null, null));
                        }
                        return opts;
                    }
                }
                return null;

            };
            KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>> curSec = new KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>(zombieCondition, zombieFunc);
            floatMenus.Add(curSec);
            return floatMenus;
        }
    }
}