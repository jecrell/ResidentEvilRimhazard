// RimWorld.JobDriver_Execute
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RERimhazard
{



    public class JobDriver_GoToStairs : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            var teleport = new Toil();
            teleport.initAction = () =>
            {
                //var pawn = GetActor();
                //pawn.DeSpawn();
                var zLvls = Find.World.GetComponent<WorldComponent_ZLevels>();
                if (TargetA.Thing is Building_StairsUp sup)
                {
                    zLvls.AddTeleportJob(GetActor(), new TargetInfo(GetActor().Position, zLvls.GetAboveMap(sup.Map.Parent)));
                    //GenSpawn.Spawn(pawn, sup.Position, Find.World.GetComponent<WorldComponent_ZLevels>().GetAboveMap(sup.Tile, sup.Map.Parent));
                }
                if (TargetA.Thing is Building_StairsDown sdo)
                {
                    zLvls.AddTeleportJob(GetActor(), new TargetInfo(GetActor().Position, zLvls.GetBelowMap(sdo.Map.Parent)));
                    //GenSpawn.Spawn(pawn, sdo.Position, Find.World.GetComponent<WorldComponent_ZLevels>().GetBelowMap(sdo.Tile, sdo.Map.Parent));
                }
            };
            yield return teleport;
            yield return Toils_General.Wait(300);
        }
    }
}