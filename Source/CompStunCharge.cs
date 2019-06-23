using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    public class CompStunCharge : ThingComp
    {
        private float storedEnergy;

        public float StoredEnergy => storedEnergy;

        public float StoredEnergyMax = 100;

        public void DrainEnergy()
        {
            storedEnergy = 0;
        }

	public float StoredEnergyPct => storedEnergy / StoredEnergyMax;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            yield return new Gizmo_StunGunCharge()
            {
                stunGun = this
            };
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 8 == 0)
            {
                storedEnergy = Mathf.Clamp(storedEnergy + 1, 0, StoredEnergyMax);
            }
        }
    }
}
