using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class MapParent_ZLevel : MapParent
    {
        private int z = 0;
        public int Z => z;

        public void SetZ(int newZ) => z = newZ;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.z, "z", -1, false);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo current in base.GetGizmos())
            {
                yield return current;
            }
            yield break;
        }

        protected override bool UseGenericEnterMapFloatMenuOption
        {
            get
            {
                return false;
            }
        }

        public void Abandon()
        {
            this.shouldBeDeleted = true;
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            bool result = false;
            bool flag = this.shouldBeDeleted;
            if (flag)
            {
                result = true;
                alsoRemoveWorldObject = true;
            }
            return result;
        }

        public bool shouldBeDeleted = false;

    }
}