// RimWorld.GenStep_Power
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RERimhazard
{

    /// <summary>
    /// For the zombie related bases, power lines are randomly interrupted.
    /// </summary>
    public class GenStep_ZombiePower : GenStep
    {
        public bool canSpawnBatteries = true;

        public bool canSpawnPowerGenerators = true;

        public bool spawnRoofOverNewBatteries = true;

        public FloatRange newBatteriesInitialStoredEnergyPctRange = new FloatRange(0.2f, 0.5f);

        private List<Thing> tmpThings = new List<Thing>();

        private List<IntVec3> tmpCells = new List<IntVec3>();

        private const int MaxDistToExistingNetForTurrets = 13;

        private const int RoofPadding = 2;

        private static readonly IntRange MaxDistanceBetweenBatteryAndTransmitter = new IntRange(20, 50);

        private Dictionary<PowerNet, bool> tmpPowerNetPredicateResults = new Dictionary<PowerNet, bool>();

        private static List<IntVec3> tmpTransmitterCells = new List<IntVec3>();

        public override int SeedPart => 1186199651;

        public override void Generate(Map map, GenStepParams parms)
        {
            map.skyManager.ForceSetCurSkyGlow(1f);
            map.powerNetManager.UpdatePowerNetsAndConnections_First();
            UpdateDesiredPowerOutputForAllGenerators(map);
            EnsureBatteriesConnectedAndMakeSense(map);
            EnsurePowerUsersConnected(map);
            EnsureGeneratorsConnectedAndMakeSense(map);
            tmpThings.Clear();
        }

        private void UpdateDesiredPowerOutputForAllGenerators(Map map)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (IsPowerGenerator(tmpThings[i]))
                {
                    tmpThings[i].TryGetComp<CompPowerPlant>()?.UpdateDesiredPowerOutput();
                }
            }
        }

        private void EnsureBatteriesConnectedAndMakeSense(Map map)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                CompPowerBattery compPowerBattery = tmpThings[i].TryGetComp<CompPowerBattery>();
                if (compPowerBattery == null)
                {
                    continue;
                }
                PowerNet powerNet = compPowerBattery.PowerNet;
                if (powerNet != null && HasAnyPowerGenerator(powerNet))
                {
                    continue;
                }
                map.powerNetManager.UpdatePowerNetsAndConnections_First();
                Building newPowerGenerator2;
                if (TryFindClosestReachableNet(compPowerBattery.parent.Position, (PowerNet x) => HasAnyPowerGenerator(x), map, out PowerNet foundNet, out IntVec3 closestTransmitter))
                {
                    map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                    if (canSpawnPowerGenerators)
                    {
                        int count = tmpCells.Count;
                        IntRange maxDistanceBetweenBatteryAndTransmitter = MaxDistanceBetweenBatteryAndTransmitter;
                        float a = maxDistanceBetweenBatteryAndTransmitter.min;
                        IntRange maxDistanceBetweenBatteryAndTransmitter2 = MaxDistanceBetweenBatteryAndTransmitter;
                        float chance = Mathf.InverseLerp(a, maxDistanceBetweenBatteryAndTransmitter2.max, count);
                        if (Rand.Chance(chance) && TrySpawnPowerGeneratorNear(compPowerBattery.parent.Position, map, compPowerBattery.parent.Faction, out Building newPowerGenerator))
                        {
                            SpawnTransmitters(compPowerBattery.parent.Position, newPowerGenerator.Position, map, compPowerBattery.parent.Faction);
                            foundNet = null;
                        }
                    }
                    if (foundNet != null)
                    {
                        SpawnTransmitters(tmpCells, map, compPowerBattery.parent.Faction);
                    }
                }
                else if (canSpawnPowerGenerators && TrySpawnPowerGeneratorNear(compPowerBattery.parent.Position, map, compPowerBattery.parent.Faction, out newPowerGenerator2))
                {
                    SpawnTransmitters(compPowerBattery.parent.Position, newPowerGenerator2.Position, map, compPowerBattery.parent.Faction);
                }
            }
        }

        private void EnsurePowerUsersConnected(Map map)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (!IsPowerUser(tmpThings[i]))
                {
                    continue;
                }
                CompPowerTrader powerComp = tmpThings[i].TryGetComp<CompPowerTrader>();
                PowerNet powerNet = powerComp.PowerNet;
                if (powerNet != null && powerNet.hasPowerSource)
                {
                    TryTurnOnImmediately(powerComp, map);
                    continue;
                }
                map.powerNetManager.UpdatePowerNetsAndConnections_First();
                Building newBattery;
                if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentEnergyGainRate() - powerComp.Props.basePowerConsumption * CompPower.WattsToWattDaysPerTick > 1E-07f, map, out PowerNet foundNet, out IntVec3 closestTransmitter))
                {
                    map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                    bool flag = false;
                    if (canSpawnPowerGenerators && tmpThings[i] is Building_Turret && tmpCells.Count > 13)
                    {
                        flag = TrySpawnPowerGeneratorAndBatteryIfCanAndConnect(tmpThings[i], map);
                    }
                    if (!flag)
                    {
                        SpawnTransmitters(tmpCells, map, tmpThings[i].Faction);
                    }
                    TryTurnOnImmediately(powerComp, map);
                }
                else if (canSpawnPowerGenerators && TrySpawnPowerGeneratorAndBatteryIfCanAndConnect(tmpThings[i], map))
                {
                    TryTurnOnImmediately(powerComp, map);
                }
                else if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentStoredEnergy() > 1E-07f, map, out foundNet, out closestTransmitter))
                {
                    map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                    SpawnTransmitters(tmpCells, map, tmpThings[i].Faction);
                }
                else if (canSpawnBatteries && TrySpawnBatteryNear(tmpThings[i].Position, map, tmpThings[i].Faction, out newBattery))
                {
                    SpawnTransmitters(tmpThings[i].Position, newBattery.Position, map, tmpThings[i].Faction);
                    if (newBattery.GetComp<CompPowerBattery>().StoredEnergy > 0f)
                    {
                        TryTurnOnImmediately(powerComp, map);
                    }
                }
            }
        }

        private void EnsureGeneratorsConnectedAndMakeSense(Map map)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (!IsPowerGenerator(tmpThings[i]))
                {
                    continue;
                }
                PowerNet powerNet = tmpThings[i].TryGetComp<CompPower>().PowerNet;
                if (powerNet == null || !HasAnyPowerUser(powerNet))
                {
                    map.powerNetManager.UpdatePowerNetsAndConnections_First();
                    if (TryFindClosestReachableNet(tmpThings[i].Position, (PowerNet x) => HasAnyPowerUser(x), map, out PowerNet _, out IntVec3 closestTransmitter))
                    {
                        map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                        SpawnTransmitters(tmpCells, map, tmpThings[i].Faction);
                    }
                }
            }
        }

        private bool IsPowerUser(Thing thing)
        {
            CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
            return compPowerTrader != null && (compPowerTrader.PowerOutput < 0f || (!compPowerTrader.PowerOn && compPowerTrader.Props.basePowerConsumption > 0f));
        }

        private bool IsPowerGenerator(Thing thing)
        {
            if (thing.TryGetComp<CompPowerPlant>() != null)
            {
                return true;
            }
            CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
            return compPowerTrader != null && (compPowerTrader.PowerOutput > 0f || (!compPowerTrader.PowerOn && compPowerTrader.Props.basePowerConsumption < 0f));
        }

        private bool HasAnyPowerGenerator(PowerNet net)
        {
            List<CompPowerTrader> powerComps = net.powerComps;
            for (int i = 0; i < powerComps.Count; i++)
            {
                if (IsPowerGenerator(powerComps[i].parent))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasAnyPowerUser(PowerNet net)
        {
            List<CompPowerTrader> powerComps = net.powerComps;
            for (int i = 0; i < powerComps.Count; i++)
            {
                if (IsPowerUser(powerComps[i].parent))
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryFindClosestReachableNet(IntVec3 root, Predicate<PowerNet> predicate, Map map, out PowerNet foundNet, out IntVec3 closestTransmitter)
        {
            tmpPowerNetPredicateResults.Clear();
            PowerNet foundNetLocal = null;
            IntVec3 closestTransmitterLocal = IntVec3.Invalid;
            map.floodFiller.FloodFill(root, (IntVec3 x) => EverPossibleToTransmitPowerAt(x, map), delegate (IntVec3 x)
            {
                PowerNet powerNet = x.GetTransmitter(map)?.GetComp<CompPower>().PowerNet;
                if (powerNet == null)
                {
                    return false;
                }
                if (!tmpPowerNetPredicateResults.TryGetValue(powerNet, out bool value))
                {
                    value = predicate(powerNet);
                    tmpPowerNetPredicateResults.Add(powerNet, value);
                }
                if (value)
                {
                    foundNetLocal = powerNet;
                    closestTransmitterLocal = x;
                    return true;
                }
                return false;
            }, int.MaxValue, rememberParents: true);
            tmpPowerNetPredicateResults.Clear();
            if (foundNetLocal != null)
            {
                foundNet = foundNetLocal;
                closestTransmitter = closestTransmitterLocal;
                return true;
            }
            foundNet = null;
            closestTransmitter = IntVec3.Invalid;
            return false;
        }

        private void SpawnTransmitters(List<IntVec3> cells, Map map, Faction faction)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].GetTransmitter(map) == null)
                {
                    if (Rand.Value > 0.1)
                    {
                        Thing thing = GenSpawn.Spawn(ThingDefOf.PowerConduit, cells[i], map);
                        thing.SetFaction(faction);
                    }
                }
            }
        }

        private void SpawnTransmitters(IntVec3 start, IntVec3 end, Map map, Faction faction)
        {
            bool foundPath = false;
            map.floodFiller.FloodFill(start, (IntVec3 x) => EverPossibleToTransmitPowerAt(x, map), delegate (IntVec3 x)
            {
                if (x == end)
                {
                    foundPath = true;
                    return true;
                }
                return false;
            }, int.MaxValue, rememberParents: true);
            if (foundPath)
            {

                map.floodFiller.ReconstructLastFloodFillPath(end, tmpTransmitterCells);
                SpawnTransmitters(tmpTransmitterCells, map, faction);
            }
        }

        private bool TrySpawnPowerTransmittingBuildingNear(IntVec3 position, Map map, Faction faction, ThingDef def, out Building newBuilding, Predicate<IntVec3> extraValidator = null)
        {
            TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassAllDestroyableThings);
            if (RCellFinder.TryFindRandomCellNearWith(position, delegate (IntVec3 x)
            {
                if (!x.Standable(map) || x.Roofed(map) || !EverPossibleToTransmitPowerAt(x, map))
                {
                    return false;
                }
                if (!map.reachability.CanReach(position, x, PathEndMode.OnCell, traverseParams))
                {
                    return false;
                }
                CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(x, Rot4.North, def.size).GetIterator();
                while (!iterator.Done())
                {
                    IntVec3 current = iterator.Current;
                    if (!current.InBounds(map) || current.Roofed(map) || current.GetEdifice(map) != null || current.GetFirstItem(map) != null || current.GetTransmitter(map) != null)
                    {
                        return false;
                    }
                    iterator.MoveNext();
                }
                if (extraValidator != null && !extraValidator(x))
                {
                    return false;
                }
                return true;
            }, map, out IntVec3 result, 8))
            {
                newBuilding = (Building)GenSpawn.Spawn(ThingMaker.MakeThing(def), result, map, Rot4.North);
                newBuilding.SetFaction(faction);
                return true;
            }
            newBuilding = null;
            return false;
        }

        private bool TrySpawnPowerGeneratorNear(IntVec3 position, Map map, Faction faction, out Building newPowerGenerator)
        {
            if (TrySpawnPowerTransmittingBuildingNear(position, map, faction, ThingDefOf.SolarGenerator, out newPowerGenerator))
            {
                map.powerNetManager.UpdatePowerNetsAndConnections_First();
                newPowerGenerator.GetComp<CompPowerPlant>().UpdateDesiredPowerOutput();
                return true;
            }
            return false;
        }

        private bool TrySpawnBatteryNear(IntVec3 position, Map map, Faction faction, out Building newBattery)
        {
            Predicate<IntVec3> extraValidator = null;
            if (spawnRoofOverNewBatteries)
            {
                extraValidator = delegate (IntVec3 x)
                {
                    CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(x, Rot4.North, ThingDefOf.Battery.size).ExpandedBy(3).GetIterator();
                    while (!iterator.Done())
                    {
                        IntVec3 current = iterator.Current;
                        if (current.InBounds(map))
                        {
                            List<Thing> thingList = current.GetThingList(map);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                if (thingList[i].def.PlaceWorkers != null && thingList[i].def.PlaceWorkers.Any((PlaceWorker y) => y is PlaceWorker_NotUnderRoof))
                                {
                                    return false;
                                }
                            }
                        }
                        iterator.MoveNext();
                    }
                    return true;
                };
            }
            if (TrySpawnPowerTransmittingBuildingNear(position, map, faction, ThingDefOf.Battery, out newBattery, extraValidator))
            {
                float randomInRange = newBatteriesInitialStoredEnergyPctRange.RandomInRange;
                newBattery.GetComp<CompPowerBattery>().SetStoredEnergyPct(randomInRange);
                if (spawnRoofOverNewBatteries)
                {
                    SpawnRoofOver(newBattery);
                }
                return true;
            }
            return false;
        }

        private bool TrySpawnPowerGeneratorAndBatteryIfCanAndConnect(Thing forThing, Map map)
        {
            if (!canSpawnPowerGenerators)
            {
                return false;
            }
            IntVec3 position = forThing.Position;
            if (canSpawnBatteries)
            {
                float chance = (!(forThing is Building_Turret)) ? 0.1f : 1f;
                if (Rand.Chance(chance) && TrySpawnBatteryNear(forThing.Position, map, forThing.Faction, out Building newBattery))
                {
                    SpawnTransmitters(forThing.Position, newBattery.Position, map, forThing.Faction);
                    position = newBattery.Position;
                }
            }
            if (TrySpawnPowerGeneratorNear(position, map, forThing.Faction, out Building newPowerGenerator))
            {
                SpawnTransmitters(position, newPowerGenerator.Position, map, forThing.Faction);
                return true;
            }
            return false;
        }

        private bool EverPossibleToTransmitPowerAt(IntVec3 c, Map map)
        {
            return c.GetTransmitter(map) != null || GenConstruct.CanBuildOnTerrain(ThingDefOf.PowerConduit, c, map, Rot4.North);
        }

        /// <summary>
        /// For zombies, we don't want the power actually turned on immediately.
        /// </summary>
        /// <param name="powerComp"></param>
        /// <param name="map"></param>
        private void TryTurnOnImmediately(CompPowerTrader powerComp, Map map)
        {
            // Do nothing.

            //if (!powerComp.PowerOn)
            //{
            //    map.powerNetManager.UpdatePowerNetsAndConnections_First();
            //    if (powerComp.PowerNet != null && powerComp.PowerNet.CurrentEnergyGainRate() > 1E-07f)
            //    {
            //        powerComp.PowerOn = true;
            //    }
            //}
        }

        private void SpawnRoofOver(Thing thing)
        {
            CellRect cellRect = thing.OccupiedRect();
            bool flag = true;
            CellRect.CellRectIterator iterator = cellRect.GetIterator();
            while (!iterator.Done())
            {
                if (!iterator.Current.Roofed(thing.Map))
                {
                    flag = false;
                    break;
                }
                iterator.MoveNext();
            }
            if (flag)
            {
                return;
            }
            int num = 0;
            CellRect cellRect2 = cellRect.ExpandedBy(2);
            CellRect.CellRectIterator iterator2 = cellRect2.GetIterator();
            while (!iterator2.Done())
            {
                if (iterator2.Current.InBounds(thing.Map) && iterator2.Current.GetRoofHolderOrImpassable(thing.Map) != null)
                {
                    num++;
                }
                iterator2.MoveNext();
            }
            if (num < 2)
            {
                ThingDef stuff = Rand.Element(ThingDefOf.WoodLog, ThingDefOf.Steel);
                foreach (IntVec3 corner in cellRect2.Corners)
                {
                    if (corner.InBounds(thing.Map) && corner.Standable(thing.Map) && corner.GetFirstItem(thing.Map) == null && corner.GetFirstBuilding(thing.Map) == null && corner.GetFirstPawn(thing.Map) == null && !GenAdj.CellsAdjacent8Way(new TargetInfo(corner, thing.Map)).Any((IntVec3 x) => !x.InBounds(thing.Map) || !x.Walkable(thing.Map)) && corner.SupportsStructureType(thing.Map, ThingDefOf.Wall.terrainAffordanceNeeded))
                    {
                        Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Wall, stuff);
                        GenSpawn.Spawn(thing2, corner, thing.Map);
                        thing2.SetFaction(thing.Faction);
                        num++;
                    }
                }
            }
            if (num <= 0)
            {
                return;
            }
            CellRect.CellRectIterator iterator3 = cellRect2.GetIterator();
            while (!iterator3.Done())
            {
                if (iterator3.Current.InBounds(thing.Map) && !iterator3.Current.Roofed(thing.Map))
                {
                    thing.Map.roofGrid.SetRoof(iterator3.Current, RoofDefOf.RoofConstructed);
                }
                iterator3.MoveNext();
            }
        }
    }


}
