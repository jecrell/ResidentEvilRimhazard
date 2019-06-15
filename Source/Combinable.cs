using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private Gizmo GetCombineGizmo(Thing objectA)
        {
            Command_Target command_Target = new Command_Target();
            command_Target.defaultLabel = "RE_Combine".Translate();
            command_Target.defaultDesc = "RE_CombineDesc".Translate();
            command_Target.targetingParams = new TargetingParameters() { canTargetItems = true, canTargetBuildings = false, canTargetPawns = false, canTargetSelf = false, mapObjectTargetsMustBeAutoAttackable = false};
            command_Target.hotKey = KeyBindingDefOf.Misc1;
            command_Target.icon = TexButton.Combine;
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
