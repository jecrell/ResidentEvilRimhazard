using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RERimhazard
{

    public class MapComponent_ZombieTracker : MapComponent
    {
        private Dictionary<IntVec3, Pawn> infectedDeadLocations = new Dictionary<IntVec3, Pawn>();
        private Dictionary<Pawn, int> deadToRise = new Dictionary<Pawn, int>();

        public void AddInfectedDeadLocation(Pawn infectedDead)
        {
            List<IntVec3> locations = new List<IntVec3>();
            if (infectedDead.Corpse == null) locations.AddRange(infectedDead.CellsAdjacent8WayAndInside());
            else
                locations.AddRange(infectedDead.Corpse.CellsAdjacent8WayAndInside());
            foreach (var loc in locations)
            {
                if (!infectedDeadLocations.Keys.Contains(loc))
                {
                    //Log.Message($"IDL: {infectedDead.Label} : {loc.x},{loc.z}");
                    infectedDeadLocations.Add(loc, infectedDead);
                }
            }
        }

        public void Notify_ZombieDied(Pawn zombie)
        {
            if (Rand.Value < RESettings.DEADZOMBIE_RESURRECTION_CHANCE)
            {
                deadToRise.Add(zombie, Find.TickManager.TicksGame + Rand.Range(RESettings.DEADZOMBIE_RESURRECTION_MINTIME, RESettings.DEADZOMBIE_RESURRECTION_MAXTIME));
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            //Bring back some of the dead
            if (Find.TickManager.TicksGame % 500 == 0)
            {
                if (deadToRise != null && deadToRise?.Count > 0)
                {
                    HashSet<Pawn> risen = new HashSet<Pawn>();
                    foreach(var dtr in deadToRise)
                    {
                        if (dtr.Value > Find.TickManager.TicksGame)
                        {
                            ResurrectionUtility.Resurrect(dtr.Key);
                        }
                    }
                    if (risen != null && risen?.Count() > 0)
                    {
                        foreach (var r in risen)
                        {
                            deadToRise.RemoveAll(x => x.Key == r);
                        }
                    }
                }
            }

            //Only perform this action after a certain time 
            if (Find.TickManager.TicksGame % RESettings.RESSURECTION_TIME == 0)
            {
                //Log.Message($"ZT Tick : { Find.TickManager.TicksGame}");
                //If no infected dead locations exist, we shouldn't continue
                if (infectedDeadLocations == null || infectedDeadLocations.Count <= 0) return;

                //Log.Message($"ZT Pass 1");

                //Clean out destroyed corpses, and then exit if none remain
                infectedDeadLocations.RemoveAll(x => 
                    x.Value == null);
                if (infectedDeadLocations == null || infectedDeadLocations.Count <= 0) return;


                //Log.Message($"ZT Pass 2");

                //Check all active humanoid pawns. 
                //  If any are setting foot on infectedDeadLocations,
                //    then trigger zombies based on percentages.

                var humanoids =
                    (from p in map.mapPawns.AllPawnsSpawned
                    where !p.NonHumanlikeOrWildMan()
                    select p).ToArray();


                //Log.Message($"ZT Pass 3");


                //Track new resurrections
                HashSet<Pawn> toBeResurrected = new HashSet<Pawn>();
                for (int i = 0; i < humanoids?.Count(); i++)
                {
                    var humanoid = humanoids[i];
                    if (toBeResurrected.Contains(humanoid)) continue;
                    if (infectedDeadLocations.Keys.Contains(humanoid.PositionHeld))
                    {
                        ////30% chance of resurrection
                        //if (Rand.Range(0.0f, 1.0f) < 0.6f)
                        //    continue;

                        //Add zombie to be resurrected
                        var newZombie = infectedDeadLocations[humanoid.PositionHeld];
                        toBeResurrected.Add(newZombie);
                    }
                }


                //Log.Message($"ZT Pass 4");


                //Resurrect each lucky new zombie
                if (toBeResurrected == null || toBeResurrected?.Count <= 0) return;
                foreach (var newZombie in toBeResurrected)
                {

                    //Log.Message($"ZT {newZombie.Label} resurected");

                    ZombieUtility.CreateZombieAtSourcePawnLocation(newZombie);
                    infectedDeadLocations.RemoveAll(x => x.Value == newZombie);
                }


                //Log.Message($"ZT Pass Complete");

            }
        }

        public MapComponent_ZombieTracker(Map map) : base(map)
        {
            this.map = map;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
