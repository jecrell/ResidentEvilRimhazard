using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class CombinableSyringeEmpty : CombinableSyringe
    {
        public override bool CanCombine(Thing a, Thing b)
        {
            if (b is Zombie || b is BOW)
                return true;
            return false;
        }

        public override void InjectionEffect(Pawn target)
        {
            var syringeType = "";
            var targetKind = target.kindDef.defName;
            if (targetKind == "RE_GKind")
                syringeType = "RE_SyringeRE_GVirusScratch";
            if (targetKind == "RE_ZombieKind" ||
                targetKind == "RE_TyrantKind" ||
                targetKind == "RE_TyrantOneZeroThreeKind" ||
                targetKind == "RE_ZombieDogKind" ||
                targetKind == "RE_LickerKind")
                syringeType = "RE_SyringeRE_TVirusScratch";
            if (syringeType == "")
                return;
            var newSyringe = ThingMaker.MakeThing(ThingDef.Named(syringeType));
            GenPlace.TryPlaceThing(newSyringe, target.PositionHeld, target.MapHeld, ThingPlaceMode.Near);
            target.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 1, 1, -1, null, target.health.hediffSet.GetNotMissingParts().First(x => x.def == BodyPartDefOf.Torso)));
        }
    }
}
