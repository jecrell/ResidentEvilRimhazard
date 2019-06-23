using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.Sound;

namespace RERimhazard
{
    public class PawnRelocatable : Pawn
    {
        public bool isMoving = false;

        IntVec3 tempLoc;

        public void ProcessInput()
        {
            if (!this.isMoving)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                Map map = this.Map;
                List<Pawn> colonists = new List<Pawn>(map.mapPawns.FreeColonistsSpawned);
                if (colonists.Count != 0)
                {
                    foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!current.Dead)
                        {
                            string text = current.Name.ToStringFull;
                            List<FloatMenuOption> arg_121_0 = list;
                            Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, current);
                            arg_121_0.Add(new FloatMenuOption(text, delegate
                            {
                                this.TryHaulZombie(current);
                            }, MenuOptionPriority.Default, null, null, 29f, extraPartOnGUI, null));
                        }
                    }
                }
                else
                {
                    list.Add(new FloatMenuOption("Nocolonists".Translate(), delegate
                    {
                    }, MenuOptionPriority.Default));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            else
            {
                TryCancelHaul();
            }
        }

        private void TryCancelHaul(string reason = "")
        {
            Pawn pawn = null;
            List<Pawn> listeners = this.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            bool[] flag = new bool[listeners.Count];
            for (int i = 0; i < listeners.Count; i++)
            {
                pawn = listeners[i];
                if (pawn.Faction == Faction.OfPlayer)
                {
                    if (pawn.CurJob.def == DefDatabase<JobDef>.GetNamed("RE_HaulZombie"))
                    {
                        pawn.jobs.StopAll();
                    }
                }
            }
            tempLoc = IntVec3.Invalid;
            this.isMoving = false;
            Messages.Message("RE_CancelRelocation".Translate(reason), MessageTypeDefOf.NegativeEvent);
        }


        public static bool IsActorAvailable(Pawn preacher, bool downedAllowed = false)
        {
            if (preacher == null)
                return false;
            if (preacher.Dead)
                return false;
            if (preacher.Downed && !downedAllowed)
                return false;
            if (preacher.Drafted)
                return false;
            if (preacher.InAggroMentalState)
                return false;
            if (preacher.InMentalState)
                return false;
            return true;
        }


        private void StartHaul(Pawn actor)
        {
            if (this.Destroyed || !this.Spawned)
            {
                TryCancelHaul("RE_Unavailable".Translate());
                return;
            }
            if (!IsActorAvailable(actor))
            {
                TryCancelHaul("RE_ColonistUnavailable".Translate());
                return;
            }



            var targetingParams = new TargetingParameters()
            {
                canTargetLocations = true,
                validator = ((TargetInfo c) => c.Cell is IntVec3 cell && cell.IsValid && cell.Standable(c.Map)),
            };

            //var texture2D = (Texture2D)GhostUtility.GhostGraphicFor(this.def.graphicData.Graphic, this.def, Color.white).MatSouth.GetTexture(ShaderPropertyIDs.Color);

            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo target)
            {
                tempLoc = target.Cell;
                this.isMoving = true;
                MakeHaulJobZombie(actor, target.Cell);
                Find.Targeter.StopTargeting();
            });

        }

        private void MakeHaulJobZombie(Pawn actor, IntVec3 loc)
        {
            Job job = new Job(DefDatabase<JobDef>.GetNamed("RE_HaulZombie"), this, loc)
            {
                count = 1
            };
            actor.jobs.TryTakeOrderedJob(job);
        }

        private void TryHaulZombie(Pawn actor)
        {

            if (actor != null)
            {
                StartHaul(actor);
            }
            else
            {
                Messages.Message("RE_CannotFindActor".Translate(), MessageTypeDefOf.RejectInput);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }

            if (this.Downed)
            {
                if (!this.isMoving)
                {
                    Command_Action command_Action = new Command_Action()
                    {
                        action = new Action(this.ProcessInput),
                        defaultLabel = "RE_RelocateZombie".Translate(this.Label),
                        defaultDesc = "RE_RelocateZombieDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc1,
                        icon = TexCommand.Install //ContentFinder<Texture2D>.Get("UI/Commands/Forcolonists", true)
                    };
                    yield return command_Action;
                }
                else
                {
                    Command_Action command_Cancel = new Command_Action()
                    {
                        action = new Action(this.ProcessInput),
                        defaultLabel = "CommandCancelConstructionLabel".Translate(),
                        defaultDesc = "RE_CommandCancelRelocation".Translate(),
                        hotKey = KeyBindingDefOf.Designator_Cancel,
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true)
                    };
                    yield return command_Cancel;
                }
            }

        }

    }
}
