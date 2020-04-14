using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RERimhazard
{
    public class GameComponent_Rimhazard : GameComponent
    {

        public Map underground = null;

        public GameComponent_Rimhazard(Game game)
        {

        }

        private bool firstTick = true;
        public override void GameComponentTick()
        {
            if (firstTick)
            {
                firstTick = false;
                var zombiesRemoved = false;

                //Unflood the fog
                foreach (var pawn in HarmonyPatches.startingPawns)
                {
                    Traverse.Create(pawn.Map.fogGrid).Method("FloodUnfogAdjacent", pawn.Position).GetValue();
                
                    if (!zombiesRemoved)
                    {
                        zombiesRemoved = true;
                        //No zombies at spawnpoint
                        var zombiesNearby = pawn.Map.mapPawns.AllPawnsSpawned.FindAll(p => (p is Zombie || p is BOW) && p.PositionHeld.DistanceToSquared(pawn.Position) < 10);
                        foreach (var zombie in zombiesNearby)
                        {
                            zombie.Destroy();
                        }

                        CameraJumper.TryJump(pawn);
                    }
                }

            }
        }

        public override void StartedNewGame()
        {
            Log.Message("Started new game");

            var REScenarios = new List<string>
                {
                    "Resident Evil",
                    "Resident Evil - Umbrella Corp",
                    "Resident Evil - Naked Brutality"
                };
            if (REScenarios.Any(x => x == Find.Scenario.name))
            {
                var map = underground == null ? HarmonyPatches.startingPawns.First().Map : underground;
                var pos = HarmonyPatches.startingPawns.First().Position;

                
                foreach (Pawn startingAndOptionalPawn in HarmonyPatches.startingPawns)
                {

                    if (startingAndOptionalPawn.Spawned)
                    {
                        //Give K9 backstory characters a Doberman.
                        if (startingAndOptionalPawn.Spawned &&
                        startingAndOptionalPawn.story != null &&
                        startingAndOptionalPawn.story.childhood != null &&
                        startingAndOptionalPawn.story.childhood.title == "Police Cadet (K9)")
                        {
                            var pawn = PawnGenerator.GeneratePawn(PawnKindDef.Named("RE_DobermanPinscherKind"), startingAndOptionalPawn.Faction);

                            GenPlace.TryPlaceThing(pawn, startingAndOptionalPawn.PositionHeld, startingAndOptionalPawn.MapHeld, ThingPlaceMode.Near);

                            pawn.training.Train(TrainableDefOf.Obedience, startingAndOptionalPawn, true);
                            pawn.playerSettings.Master = startingAndOptionalPawn;
                        }



                        //All characters start off aggressive
                        startingAndOptionalPawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;

                        if (Find.Scenario.name == "Resident Evil")
                            ScenarioGen.CreateBeds(startingAndOptionalPawn, startingAndOptionalPawn.Map, ThingDefOf.Bedroll, ThingDefOf.Cloth);
                    }
                    else
                    {
                        //Spawn dead bodies of other STARS members in the map.
                        CellFinder.TryFindBestPawnStandCell(startingAndOptionalPawn, out IntVec3 spot);
                        GenPlace.TryPlaceThing(startingAndOptionalPawn, spot, Find.AnyPlayerHomeMap, ThingPlaceMode.Near);
                        startingAndOptionalPawn.Kill(null);
                    }

                }


                Current.Game.CurrentMap = map;


                //FloodFillerFog.FloodUnfog(pos, map);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.firstTick, "firstTick");
        }
    }
}
