using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RERimhazard
{
    public class Combinable : ThingWithComps
    {
        public virtual bool CanCombine(Thing a, Thing b)
        {
            return false;
        }

        public virtual bool DestroysSelf()
        {
            return true;
        }

        public virtual string GetLabel()
        {
            return "RE_Combine".Translate();
        }

        public virtual string GetDescription()
        {
            return "RE_CombineDesc".Translate();
        }

        public virtual JobDef GetJobDef()
        {
            return null;
        }

        public virtual TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters() { canTargetItems = true, canTargetBuildings = false, canTargetPawns = false, canTargetSelf = false, mapObjectTargetsMustBeAutoAttackable = false };
        }

        public virtual Texture2D GetIcon()
        {
            return TexButton.Combine;
        }


        private Gizmo GetCombineGizmo(Thing objectA)
        {
            Command_Target command_Target = new Command_Target();
            command_Target.defaultLabel = GetLabel();
            command_Target.defaultDesc = GetDescription();
            command_Target.targetingParams = GetTargetingParameters();
            command_Target.hotKey = KeyBindingDefOf.Misc1;
            command_Target.icon = GetIcon();
            command_Target.action = delegate (Thing objectB)
            {
                if (CanCombine(objectA, objectB))
                {
                    CombineAction(objectA, objectB);
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
            };
            return command_Target;
        }
        
        public virtual ThingDef GetCombineResult(string one, string two)
        {
            return null;
        }

        public virtual void CombineAction(Thing a, Thing b) { return; }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            yield return GetCombineGizmo(this);
        }
    }
}
