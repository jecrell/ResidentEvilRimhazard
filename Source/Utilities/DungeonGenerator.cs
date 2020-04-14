using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    public class DungeonNode
    {
        public DungeonNode parent;
        public DungeonNode left;
        public DungeonNode right;
        public CellRect cells;
        public CellRect room;
        public int depth;
        public bool connected;
        public int numOfHallways = 0;

        public DungeonNode(CellRect newCells)
        {
            cells = newCells;
        }

        public CellRect GetRoomWithIntVec3(IntVec3 test)
        {
            var rooms = SubordinateRooms(this, new HashSet<CellRect>());
            //Log.Message($"Subordinate rooms count: {rooms.Count()}");
            foreach (var room in rooms)
            {
                if (room.Cells.Contains(test))
                {
                    if (room == default(CellRect))
                        //Log.Message("Oh my fookin christ");
                    return room;
                }
            }
            return default(CellRect);
        }

        public HashSet<CellRect> SubordinateRooms(DungeonNode cur, HashSet<CellRect> curList)
        {
            if (cur.left == null || cur.right == null)
                return curList;
            if (cur.left.room != default(CellRect))
                curList.Add(cur.left.room);
            if (cur.right.room != default(CellRect))
                curList.Add(cur.right.room);
            curList.AddRange(SubordinateRooms(cur.left, curList));
            curList.AddRange(SubordinateRooms(cur.right, curList));
            return curList;
        }
    }

    enum SpawnType : int
    {
        Nothing = 0,
        Floor = 1,
        Wall = 2,
        Door = 3,
        HallwayFloor = 4,
        HallwayWall = 5
    }

    public class DungeonGenerator
    {
        int minRoomX = 8;
        int minRoomZ = 8;
        int maxRoomX = 50;
        int maxRoomZ = 50;

        int maxDepth;

        public DungeonNode baseRoot;
        public Map map = null;
        CellRect startingRoomRect;

        List<CellRect> hallwaysAndRooms = new List<CellRect>();
        char[,] debugMap;
        
        Dictionary<IntVec3, SpawnType> cellPainter = new Dictionary<IntVec3, SpawnType>();

        public DungeonGenerator(Faction fac, IntVec3 pos, Map newMap, int numOfLayers)
        {
            map = newMap;
            maxDepth = numOfLayers;

            int maxX = map.Center.x * 2;
            int maxZ = map.Center.z * 2;
            int xDiff = (int)(maxX * 0.1f);
            int zDiff = (int)(maxZ * 0.1f);
            debugMap = new char[map.Size.z, map.Size.x];
            for (int z = 0; z < map.Size.z; z++)
            {
                for (int x = 0; x < map.Size.x; x++)
                {
                    debugMap[z, x] = '.';
                }
            }

            foreach (IntVec3 unusedCell in map.AllCells)
            {
                if (cellPainter.ContainsKey(unusedCell))
                    continue;
                cellPainter.Add(unusedCell, SpawnType.Nothing);
            }

            CellRect newRect = new CellRect(xDiff, zDiff, maxX - (xDiff), maxZ - (zDiff));
            CreateDungeonLayers(1, numOfLayers, baseRoot, newRect);
            CreateDungeonHallways(baseRoot);
            
            var rooms = GetLeaves(baseRoot, new List<DungeonNode>());
            var startingRoom = rooms.FirstOrDefault(x => x.cells.Width >= 24 && x.cells.Height >= 24);
            if (startingRoom == null) startingRoom = rooms.Last();
            foreach (var node in rooms)
            {
                RoomGeneration(node, node == startingRoom);
            }
            foreach (var node in hallwaysAndRooms)
            {
                for (int i = 0; i < 4; i++)
                    TryMakeADoor(node);
            }
            PaintAll();
            var roomsInRandomOrder = rooms.InRandomOrder();
            ScenarioGen.CreateStartingArea(fac, map, pos, startingRoomRect);
            ApplyRoomDefsToRooms(roomsInRandomOrder.ToList());

            StringBuilder s = new StringBuilder();
            for (int x2 = map.Size.x - 1; x2 > 1; x2--)
            {
                for (int z2 = 0; z2 < map.Size.z; z2++)
                {
                    s.Append(debugMap[z2, x2]);
                }
                s.AppendLine();
            }
            Log.Message(s.ToString());
        }

        public void ApplyRoomDefsToRooms(List<DungeonNode> nodes)
        {

            Dictionary<RoomGenDef, int> roomsToSpawn = new Dictionary<RoomGenDef, int>();
            foreach (RoomGenDef rgd in DefDatabase<RoomGenDef>.AllDefs)
                roomsToSpawn.Add(rgd, rgd.maxNumOfRoom);


            foreach (var node in nodes)
            {
                if (roomsToSpawn.Count == 0) break;

                var currentSpawnChoice = roomsToSpawn.First().Key;
                if (roomsToSpawn[currentSpawnChoice] == 1)
                {
                    roomsToSpawn.Remove(currentSpawnChoice);
                }
                else
                {
                    if (roomsToSpawn.Count > 1)
                        roomsToSpawn = roomsToSpawn.OrderBy(x => x.Value).ToDictionary(y => y.Key, y => y.Value);
                    roomsToSpawn[currentSpawnChoice]--;
                }
                ApplyRoomDefToRoom(node, currentSpawnChoice);
            }


        }

        public void SpawnBuildings(List<BuildingToSpawn> bldsToSpawn, DungeonNode node, ref bool hasCenterpiece)
        {
            if (bldsToSpawn?.Count > 0)
            {
                foreach (var building in bldsToSpawn)
                {
                    if (building.isCenterpiece && hasCenterpiece)
                        continue;

                    //Spawn the buildings
                    for (int i = 0; i < building.numToSpawn.RandomInRange; i++)
                    {
                        //Can the building be spawned at this node? 
                        if (building.buildingDef.Size.x >= node.room.Width - 2 ||
                            building.buildingDef.Size.z >= node.room.Height - 2)
                            break;

                        var rot = building.buildingDef.rotatable ? Rot4.Random : building.buildingDef.defaultPlacingRot;

                        IntVec3 spawnPoint =
                            (building.isCenterpiece) ?
                            node.room.CenterCell 
                                    :
                            CellFinder.FindNoWipeSpawnLocNear(
                                node.room.ContractedBy(
                                    Mathf.Max(building.buildingDef.Size.x,
                                              building.buildingDef.Size.z)).RandomCell,
                                    map, building.buildingDef, rot, (int)(node.cells.Width * 0.3f));


                        if (building.buildingDef == ThingDefOf.GeothermalGenerator)
                        {
                            var powerSource = ThingMaker.MakeThing(ThingDefOf.SteamGeyser);
                            GenPlace.TryPlaceThing(powerSource, spawnPoint, map, ThingPlaceMode.Direct);
                        }

                        var stuffDef = building.buildingDef.MadeFromStuff && building.stuffDef == null ? ThingDefOf.Steel : building.stuffDef;
                        var buildingSpawned = ThingMaker.MakeThing(building.buildingDef, stuffDef);
                        GenPlace.TryPlaceThing(buildingSpawned, spawnPoint, map, ThingPlaceMode.Near, null, null, rot);

                        if (building.isCenterpiece)
                            hasCenterpiece = true;

                        if (buildingSpawned is Plant plant)
                        {
                            plant.Growth = 1f;
                        }

                        if (building.chairDef != null)
                        {
                            var chairStuffDef = building.chairDef.MadeFromStuff ? ThingDefOf.Steel : stuffDef;
                            var chairSpawned = ThingMaker.MakeThing(building.chairDef, chairStuffDef);
                            GenPlace.TryPlaceThing(chairSpawned, buildingSpawned.InteractionCell, map, ThingPlaceMode.Direct, null, null, buildingSpawned.Rotation);
                        }

                        if (buildingSpawned?.TryGetComp<CompRefuelable>() is CompRefuelable compRefuelable)
                        {
                            Traverse.Create(compRefuelable).Field("fuel").SetValue(compRefuelable.Props.fuelCapacity);
                            ((ThingWithComps)buildingSpawned).BroadcastCompSignal("Refueled");
                        }
                    }
                }
            }
        }

        public void ApplyRoomDefToRoom(DungeonNode node, RoomGenDef rgd)
        {

            Log.Message($"{rgd.defName} for {node.room.CenterCell.ToString()}");

            bool hasCenterpiece = false;


            var minWallSize = Mathf.Min(node.room.Width, node.room.Height);
            if (minWallSize <= 5)
            {
                SpawnBuildings(rgd.furnishingsDef.tinyRoomBuildingsToSpawn, node, ref hasCenterpiece);
            }
            else if (minWallSize <= 8)
            {
                SpawnBuildings(rgd.furnishingsDef.tinyRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.smallRoomBuildingsToSpawn, node, ref hasCenterpiece);
            }
            else if (minWallSize <= 12)
            {
                SpawnBuildings(rgd.furnishingsDef.tinyRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.smallRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.mediumRoomBuildingsToSpawn, node, ref hasCenterpiece);
            }
            else if (minWallSize <= 24)
            {
                SpawnBuildings(rgd.furnishingsDef.tinyRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.smallRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.mediumRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.largeRoomBuildingsToSpawn, node, ref hasCenterpiece);
            }
            else
            {
                SpawnBuildings(rgd.furnishingsDef.tinyRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.smallRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.mediumRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.largeRoomBuildingsToSpawn, node, ref hasCenterpiece);
                SpawnBuildings(rgd.furnishingsDef.colossalRoomBuildingsToSpawn, node, ref hasCenterpiece);
            }

            if (rgd.pawnsToSpawn?.Count > 0)
            {
                foreach (var pawnKind in rgd.pawnsToSpawn)
                {
                    for (int i = 0; i < pawnKind.numToSpawn.RandomInRange; i++)
                    {
                        IntVec3 spawnPoint;
                        CellFinder.TryFindRandomCellInsideWith(node.room, ((IntVec3 x) => { return x.Standable(map); }), out spawnPoint);
                        var pawn = PawnGenerator.GeneratePawn(pawnKind.pawnKindDef, null);
                        GenSpawn.Spawn(pawn, spawnPoint, map);
                        if (pawnKind.spawnDead)
                            pawn.Kill(null);
                        if (pawnKind.factionDef != null)
                            pawn.SetFactionDirect(Find.FactionManager.FirstFactionOfDef(pawnKind.factionDef));
                    }
                }
            }


        }

        public static ThingDef RockDefAt(IntVec3 c)
        {
            ThingDef thingDef = null;
            float num = -999999f;
            for (int i = 0; i < HarmonyPatches.tempRockNoises.Count; i++)
            {
                float value = HarmonyPatches.tempRockNoises[i].noise.GetValue(c);
                bool flag = value > num;
                if (flag)
                {
                    thingDef = HarmonyPatches.tempRockNoises[i].rockDef;
                    num = value;
                }
            }
            bool flag2 = thingDef == null;
            if (flag2)
            {
                Log.ErrorOnce("Did not get rock def to generate at " + c, 50812, false);
                thingDef = ThingDefOf.Sandstone;
            }
            return thingDef;
        }

        HashSet<IntVec3> visited = new HashSet<IntVec3>();
        
        private bool WallHasDoor(CellRect rect, Rot4 dir)
        {
            foreach (IntVec3 edgeCell in rect.GetEdgeCells(dir))
            {
                if (cellPainter.ContainsKey(edgeCell) && cellPainter[edgeCell] == SpawnType.Door)
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryFindRandomDoorSpawnCell(CellRect rect, Rot4 dir, out IntVec3 found)
        {
            if (dir == Rot4.North)
            {
                if (rect.Width <= 2)
                {
                    found = IntVec3.Invalid;
                    return false;
                }
                if (!Rand.TryRangeInclusiveWhere(rect.minX + 1, rect.maxX - 1, delegate (int x)
                {
                    IntVec3 c7 = new IntVec3(x, 0, rect.maxZ + 1);
                    IntVec3 c8 = new IntVec3(x, 0, rect.maxZ - 1);
                    IntVec3 wallSpot = new IntVec3(x, 0, rect.maxZ);

                    var result = c7.InBounds(map) && 
                    (cellPainter[c7] == SpawnType.Floor ||
                    cellPainter[c7] == SpawnType.HallwayFloor) && c8.InBounds(map) && 
                    (cellPainter[c8] == SpawnType.Floor || 
                    cellPainter[c8] == SpawnType.HallwayFloor) && wallSpot.InBounds(map) &&
                    (cellPainter[wallSpot] == SpawnType.Wall || cellPainter[wallSpot] == SpawnType.HallwayWall);
                    return result;
                }, out int value))
                {
                    found = IntVec3.Invalid;
                    return false;
                }
                found = new IntVec3(value, 0, rect.maxZ);
                return true;
            }
            if (dir == Rot4.South)
            {
                if (rect.Width <= 2)
                {
                    found = IntVec3.Invalid;
                    return false;
                }
                if (!Rand.TryRangeInclusiveWhere(rect.minX + 1, rect.maxX - 1, delegate (int x)
                {
                    IntVec3 c5 = new IntVec3(x, 0, rect.minZ - 1);
                    IntVec3 c6 = new IntVec3(x, 0, rect.minZ + 1);
                    IntVec3 wallSpot = new IntVec3(x, 0, rect.minZ);
                    return c5.InBounds(map) &&
                    (cellPainter[c5] == SpawnType.Floor ||
                    cellPainter[c5] == SpawnType.HallwayFloor) && c6.InBounds(map) &&
                    (cellPainter[c6] == SpawnType.Floor ||
                    cellPainter[c6] == SpawnType.HallwayFloor) && wallSpot.InBounds(map) &&
                    (cellPainter[wallSpot] == SpawnType.Wall || cellPainter[wallSpot] == SpawnType.HallwayWall);
                }, out int value2))
                {
                    found = IntVec3.Invalid;
                    return false;
                }
                found = new IntVec3(value2, 0, rect.minZ);
                return true;
            }
            if (dir == Rot4.West)
            {
                if (rect.Height <= 2)
                {
                    found = IntVec3.Invalid;
                    return false;
                }
                if (!Rand.TryRangeInclusiveWhere(rect.minZ + 1, rect.maxZ - 1, delegate (int z)
                {
                    IntVec3 c3 = new IntVec3(rect.minX - 1, 0, z);
                    IntVec3 c4 = new IntVec3(rect.minX + 1, 0, z);
                    IntVec3 wallSpot = new IntVec3(rect.minX, 0, z);
                    return c3.InBounds(map) &&
                    (cellPainter[c3] == SpawnType.Floor ||
                    cellPainter[c3] == SpawnType.HallwayFloor) && c4.InBounds(map) &&
                    (cellPainter[c4] == SpawnType.Floor ||
                    cellPainter[c4] == SpawnType.HallwayFloor) && wallSpot.InBounds(map) &&
                    (cellPainter[wallSpot] == SpawnType.Wall || cellPainter[wallSpot] == SpawnType.HallwayWall);
                }, out int value3))
                {
                    found = IntVec3.Invalid;
                    return false;
                }
                found = new IntVec3(rect.minX, 0, value3);
                return true;
            }
            if (rect.Height <= 2)
            {
                found = IntVec3.Invalid;
                return false;
            }
            if (!Rand.TryRangeInclusiveWhere(rect.minZ + 1, rect.maxZ - 1, delegate (int z)
            {
                IntVec3 c = new IntVec3(rect.maxX + 1, 0, z);
                IntVec3 c2 = new IntVec3(rect.maxX - 1, 0, z);
                IntVec3 wallSpot = new IntVec3(rect.maxX, 0, z);
                return c.InBounds(map) &&
                    (cellPainter[c] == SpawnType.Floor ||
                    cellPainter[c] == SpawnType.HallwayFloor) && c2.InBounds(map) &&
                    (cellPainter[c2] == SpawnType.Floor ||
                    cellPainter[c2] == SpawnType.HallwayFloor) && wallSpot.InBounds(map) &&
                    (cellPainter[wallSpot] == SpawnType.Wall || cellPainter[wallSpot] == SpawnType.HallwayWall);
            }, out int value4))
            {
                found = IntVec3.Invalid;
                return false;
            }
            found = new IntVec3(rect.maxX, 0, value4);
            return true;
        }

        public bool TryMakeADoor(CellRect room)
        {
            IntVec3 intVec = IntVec3.Invalid;
            for (int i = 0; i < 4; i++)
            {
                //if (WallHasDoor(room, new Rot4(i)))
                //{
                //    continue;
                //}
                for (int j = 0; j < 10; j++)
                {
                    if (!TryFindRandomDoorSpawnCell(room, new Rot4(i), out IntVec3 found))
                    {
                        //Log.Message("no door spot found");
                        continue;
                    }
                    intVec = found;
                }
            }
            if (intVec.IsValid)
            {
                //Thing thing = ThingMaker.MakeThing(ThingDefOf.Door, ThingDefOf.Steel);
                //thing.SetFaction(rp.faction);
                //GenSpawn.Spawn(thing, intVec, this.map);
                if (cellPainter.ContainsKey(intVec))
                    cellPainter[intVec] = SpawnType.Door;
                else
                    cellPainter.Add(intVec, SpawnType.Door);
                return true;
            }
            return false;
        }
        
        private bool IsIslandDoor(IntVec3 door)
        {
            var cellUp = cellPainter[new IntVec3(door.x, door.y, door.z + 1)];
            var cellDown = cellPainter[new IntVec3(door.x, door.y, door.z - 1)];
            var cellLeft = cellPainter[new IntVec3(door.x - 1, door.y, door.z)];
            var cellRight = cellPainter[new IntVec3(door.x + 1, door.y, door.z)];

            int countOfWalkables = 0;
            if (Walkable(cellUp)) countOfWalkables++;
            if (Walkable(cellDown)) countOfWalkables++;
            if (Walkable(cellLeft)) countOfWalkables++;
            if (Walkable(cellRight)) countOfWalkables++;
            
            if (countOfWalkables > 2)
                    return true;
            return false;
        }

        private bool Walkable(SpawnType type)
        {
            if (type == SpawnType.Floor ||
                type == SpawnType.HallwayFloor ||
                type == SpawnType.Nothing)
                return true;
            return false;
        }

        public bool ShouldSpawnDoor(IntVec3 door)
        {
            if (!door.IsValid)
                return false;
            if (!door.InBounds(map))
                return false;

            try
            {
                var cellUp = cellPainter[new IntVec3(door.x, door.y, door.z + 1)];
                var cellUpUp = cellPainter[new IntVec3(door.x, door.y, door.z + 2)];
                var cellDown = cellPainter[new IntVec3(door.x, door.y, door.z - 1)];
                var cellDownDown = cellPainter[new IntVec3(door.x, door.y, door.z - 2)];
                var cellLeft = cellPainter[new IntVec3(door.x - 1, door.y, door.z)];
                var cellLeftLeft = cellPainter[new IntVec3(door.x - 2, door.y, door.z)];
                var cellRight = cellPainter[new IntVec3(door.x + 1, door.y, door.z)];
                var cellRightRight = cellPainter[new IntVec3(door.x + 2, door.y, door.z)];

                bool verticalSpace = false;
                bool horizontalSpace = false;

                //Island doors should not spawn
                if (IsIslandDoor(door))
                    return false;

                //Top or bottom of triple doors should be walls
                //if ((cellUp == SpawnType.Wall || cellUp == SpawnType.HallwayWall) && cellDown == SpawnType.Door && cellDownDown == SpawnType.Door)
                //    return false;
                //
                //if ((cellDown == SpawnType.Wall || cellDown == SpawnType.HallwayWall) && cellUp == SpawnType.Door && cellUpUp == SpawnType.Door)
                //    return false;

                //Left or right of triple doors should be walls
                //if ((cellLeft == SpawnType.Wall || cellLeft == SpawnType.HallwayWall) && cellRight == SpawnType.Door && cellRightRight == SpawnType.Door)
                //    return false;
                //if ((cellRight == SpawnType.Wall || cellRight == SpawnType.HallwayWall) && cellLeft == SpawnType.Door && cellLeftLeft == SpawnType.Door)
                //    return false;


                //if vertical space exists
                if (Walkable(cellUp) && Walkable(cellDown))
                    verticalSpace = true;

                //if horizontal space exists
                if (Walkable(cellLeft) && Walkable(cellRight))
                    horizontalSpace = true;

                //No strings of doors should exist
                if (cellRight == SpawnType.Door && cellLeft == SpawnType.Door && cellLeftLeft == SpawnType.Door)
                    return false;

                if (cellLeft == SpawnType.Door && cellRight == SpawnType.Door && cellRightRight == SpawnType.Door)
                    return false;

                if (cellDown == SpawnType.Door && cellUp == SpawnType.Door && cellUpUp == SpawnType.Door)
                    return false;

                if (cellUp == SpawnType.Door && cellDown == SpawnType.Door && cellDownDown == SpawnType.Door)
                    return false;

                if (verticalSpace || horizontalSpace)
                    return true;
            }
            catch
            {

            }
            return false;

        }

        public void PaintAll()
        {
            foreach (var cell in cellPainter.Keys)
            {
                map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);

                switch (cellPainter[cell])
                {
                    case SpawnType.Nothing:
                        Thing rock = ThingMaker.MakeThing(RockDefAt(cell));
                        GenSpawn.Spawn(rock, cell, map);
                        continue;
                    case SpawnType.Floor:
                        if (cell.GetEdifice(map) is Thing t)
                            t.Destroy();
                        map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
                        continue;
                    case SpawnType.HallwayFloor:
                        if (cell.GetEdifice(map) is Thing ht)
                            ht.Destroy();
                        map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
                        continue;
                    case SpawnType.Door:
                        if (ShouldSpawnDoor(cell))
                        {
                            Thing door = ThingMaker.MakeThing(ThingDefOf.Door, ThingDefOf.Steel);
                            //thing.SetFaction(rp.faction);
                            GenSpawn.Spawn(door, cell, this.map);
                            map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
                            continue;
                        }
                        var wallD = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Plasteel);
                        GenSpawn.Spawn(wallD, cell, map);
                        continue;
                    case SpawnType.Wall:
                        var wall = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Plasteel);
                        GenSpawn.Spawn(wall, cell, map);
                        continue;
                    case SpawnType.HallwayWall:
                        var hwall = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Plasteel);
                        GenSpawn.Spawn(hwall, cell, map);
                        continue;
                }
            }
        }

        public void CreateDungeonHallways(DungeonNode cur)
        {
            if (cur.left == null || cur.right == null)
                return;
            try
            {
                ConnectDungeonHallways(cur.left, cur.right);
            }
            catch (Exception e)
            {
                Log.Message(e.ToString());
            }
            CreateDungeonHallways(cur.left);
            CreateDungeonHallways(cur.right);
        }

        public List<DungeonNode> GetLeaves(DungeonNode root, List<DungeonNode> list)
        {
            if (root.left == null && root.right == null)
            {
                var temp = new List<DungeonNode>();
                temp.AddRange(list);
                temp.Add(root);
                return temp;
            }
            else
            {
                var temp = new List<DungeonNode>();
                temp.AddRange(GetLeaves(root.left, list));
                temp.AddRange(GetLeaves(root.right, list));
                return temp;
            }
        }

        public void ConnectDungeonHallways(DungeonNode a, DungeonNode b)
        {
            bool noARoomExists = a.room == default(CellRect);
            bool noBRoomExists = b.room == default(CellRect);
            IntVec3 midA = a.cells.CenterCell; // : a.room.CenterCell;
            IntVec3 midB = b.cells.CenterCell; // : b.room.CenterCell;

            if (noARoomExists)
            {
                var rooms = a.SubordinateRooms(a, new HashSet<CellRect>());
                midA = rooms.RandomElement().CenterCell;
            }
            else
                midA = a.room.CenterCell;

            if (noBRoomExists)
            {
                var rooms = b.SubordinateRooms(b, new HashSet<CellRect>());
                midB = rooms.RandomElement().CenterCell;
            }
            else
                midB = b.room.CenterCell;

            CellRect betweenBox = new CellRect();
            betweenBox.minX = midA.x < midB.x ? midA.x : midB.x;
            betweenBox.minZ = midA.z < midB.z ? midA.z : midB.z;
            betweenBox.Width = midA.x < midB.x ? midB.x - midA.x : midA.x - midB.x;
            betweenBox.Height = midA.z < midB.z ? midB.z - midA.z : midA.z - midB.z;

            //Log.Message($"Hallway Created: {betweenBox.minX} {betweenBox.minZ} {betweenBox.Width} {betweenBox.Height}");

            CellRect horizontalRect; 
            CellRect verticalRect;

            //Case 1 - Horizontal Only
            //
            //  A===B   B===A
            //
            //if(Mathf.Abs(midA.z - midB.z) is int absDistZ && absDistZ < 4)
            if (
                (midB.z < midA.z + 4 && midB.z > midA.z - 4) ||
                (midA.z < midB.z + 4 && midA.z > midB.z - 4)
                )
            {
                verticalRect = new CellRect(betweenBox.minX, betweenBox.minZ, 3, 3);
                horizontalRect = new CellRect(betweenBox.minX, betweenBox.minZ, betweenBox.Width, new IntRange(3, 5).RandomInRange);
                foreach (var cell in verticalRect.Cells.Concat(horizontalRect.Cells))
                    debugMap[cell.x, cell.z] = '1';
            }
            //Case 2 - Vertical Only
            //   A    B
            //   ||   ||
            //    B    A
            else if (
                 (midB.x < midA.x + 4 && midB.x > midA.x - 4) ||
                 (midA.x < midB.x + 4 && midA.x > midB.x - 4)
                )
            {
                horizontalRect = new CellRect(betweenBox.minX, betweenBox.minZ, 3, 3);
                verticalRect = new CellRect(betweenBox.minX, betweenBox.minZ, new IntRange(3, 5).RandomInRange, betweenBox.Height);
                foreach (var cell in verticalRect.Cells.Concat(horizontalRect.Cells))
                    debugMap[cell.x, cell.z] = '2';
            }
            //Case 3 - L Shape
            //   A      B
            //   ||     ||
            //   LL==B  LL==A
            //
            //      OR
            //
            //         Dropped Flipped L
            //   A ==|| B==||
            //       ||    ||
            //       B      A
            else if (
                (midA.x < midB.x && midA.z > midB.z) ||
                (midB.x < midA.x && midB.z > midA.z)
                )
            {
                var hallwaySize = new IntRange(3, 5).RandomInRange;

                //L Shape
                if (Rand.Value > 0.5f)
                {
                    verticalRect = new CellRect(betweenBox.minX, betweenBox.minZ, hallwaySize, betweenBox.Height);
                    horizontalRect = new CellRect(betweenBox.minX, betweenBox.minZ, betweenBox.Width, hallwaySize);
                }
                //Dropped Flipped L
                else
                {
                    verticalRect = new CellRect(betweenBox.maxX, betweenBox.minZ, hallwaySize, betweenBox.Height);
                    horizontalRect = new CellRect(betweenBox.minX, betweenBox.maxZ, betweenBox.Width + hallwaySize-1, hallwaySize);
                }
                foreach (var cell in verticalRect.Cells.Concat(horizontalRect.Cells))
                    debugMap[cell.x, cell.z] = '3';
            }
            //Case 4 - Flipped L
            //     B     A
            //     ||    ||
            //   A==|  B==|
            //
            //      OR
            //
            //         Dropped L
            //  ||==B  ||==A
            //  ||     ||
            //  A      B
            else if (
                 (midA.x < midB.x && midA.z < midB.z) ||
                 (midB.x < midA.x && midB.z < midA.z)
                )
            {
                var hallwaySize = new IntRange(3, 5).RandomInRange;

                //Flipped L
                if (Rand.Value > 0.5f)
                {
                    verticalRect = new CellRect(betweenBox.maxX, betweenBox.minZ, hallwaySize, betweenBox.Height);
                    horizontalRect = new CellRect(betweenBox.minX, betweenBox.minZ, betweenBox.Width, hallwaySize);
                }
                //Dropped L
                else
                {
                    verticalRect = new CellRect(betweenBox.minX, betweenBox.minZ, hallwaySize, betweenBox.Height);
                    horizontalRect = new CellRect(betweenBox.minX, betweenBox.maxZ, betweenBox.Width, hallwaySize);
                }
                foreach (var cell in verticalRect.Cells.Concat(horizontalRect.Cells))
                    debugMap[cell.x, cell.z] = '4';
            }
            else
            {
                var hallwaySize = new IntRange(3, 5).RandomInRange;
                verticalRect = new CellRect(betweenBox.minX, betweenBox.minZ, hallwaySize, betweenBox.Height);
                horizontalRect = new CellRect(betweenBox.minX, betweenBox.maxX, betweenBox.Width, hallwaySize);
            }
            
            var combinedArea = horizontalRect.ContractedBy(1).Cells.Concat(verticalRect.ContractedBy(1).Cells).ToList();
            var combinedEdges = horizontalRect.EdgeCells.Concat(verticalRect.EdgeCells).ToList();

            foreach (IntVec3 cell in combinedArea)
            {
                if (cellPainter.ContainsKey(cell))
                    cellPainter[cell] = SpawnType.HallwayFloor;
                else
                    cellPainter.Add(cell, SpawnType.HallwayFloor);
            }
            foreach (IntVec3 cell in combinedEdges)
            {
                if (cellPainter.ContainsKey(cell))
                {
                    if (cellPainter[cell] != SpawnType.HallwayFloor)
                         cellPainter[cell] = SpawnType.HallwayWall;
                }
                else
                    cellPainter.Add(cell, SpawnType.HallwayWall);
            }
            a.numOfHallways++;
            b.numOfHallways++;

            ////Find two door spots
            //IntVec3 aDoor = IntVec3.Invalid;
            //if (!noARoomExists)
            //{
            //    foreach (IntVec3 cell in combinedArea)
            //    {
            //        if (a.room.Contains(cell) && cellPainter.ContainsKey(cell) && cellPainter[cell] == SpawnType.Wall)
            //        {
            //            cellPainter[cell] = SpawnType.Door;
            //            break;
            //        }
            //    }
            //}
            //if (!noBRoomExists)
            //{
            //    foreach (IntVec3 cell in combinedArea)
            //    {
            //        if (b.room.Contains(cell) && cellPainter.ContainsKey(cell) && cellPainter[cell] == SpawnType.Wall)
            //        {
            //            cellPainter[cell] = SpawnType.Door;
            //            break;
            //        }
            //    }
            //}

            hallwaysAndRooms.Add(horizontalRect);
            hallwaysAndRooms.Add(verticalRect);
        }

        public DungeonNode CreateDungeonLayers(int currentIndex, int numOfLayers, DungeonNode root, CellRect cellRect)
        {

            DungeonNode temp = new DungeonNode(cellRect);
            var currentDepth = root == null ? 1 : root.depth + 1;
            //Log.Message($"Current depth: {currentDepth.ToString()}");
            temp.parent = root;
            temp.depth = currentDepth;
            root = temp;
            if (this.baseRoot == null)
                this.baseRoot = root;

            //Create room
            if (root!= null && root.depth >= maxDepth)
            {

                RoomCellRect(root);
                if (root.room.Width < minRoomX) return root;
                if (root.room.Height < minRoomZ) return root;
                //Log.Message($"Room created at: {root.Cells.CenterCell.ToString()}");
                return root;
            }

            if (root == null || root.depth <= maxDepth)
            {
                //Split cells
                CellRect[] splitDungeon = SplitCellRects(root.cells);

                //Add left child
                root.left = CreateDungeonLayers(currentIndex + 1, numOfLayers, root, splitDungeon[0]);
                //Add right child
                root.right = CreateDungeonLayers(currentIndex + 1, numOfLayers, root, splitDungeon[1]);
            }
            return root;
        }
        

        public void RoomGeneration(DungeonNode node, bool isStartingRoom)
        {
            //Clear base space
            if (isStartingRoom)
            {
                var finalRoom = new CellRect(node.room.CenterCell.x - 12, node.room.CenterCell.z - 12, 24, 24);
                node.room = finalRoom;
                startingRoomRect = finalRoom;
            }
            CellRect rect = node.room;
            HashSet<IntVec3> edgeCells = new HashSet<IntVec3>(rect.EdgeCells);
            HashSet<IntVec3> innerCells = new HashSet<IntVec3>(rect.ContractedBy(1).Cells);
            foreach (IntVec3 cell in innerCells)
            {
                if (cellPainter.ContainsKey(cell))
                {
                    if (cellPainter[cell] != SpawnType.Door)
                        cellPainter[cell] = SpawnType.Floor;
                }
                else
                    cellPainter.Add(cell, SpawnType.Floor);
            }
            foreach (IntVec3 cell in edgeCells)
            {
                if (cellPainter.ContainsKey(cell))
                {
                    if (cellPainter[cell] == SpawnType.Door)
                        continue;
                    //if (cellPainter[cell] == SpawnType.HallwayWall)
                    //{
                    //    cellPainter[cell] = SpawnType.Door;
                    //    continue;
                    //}
                    if (cellPainter[cell] == SpawnType.HallwayFloor)
                    {
                        cellPainter[cell] = SpawnType.Door;
                        continue;
                    }
                    cellPainter[cell] = SpawnType.Wall;
                }
                else
                    cellPainter.Add(cell, SpawnType.Wall);
            }
            hallwaysAndRooms.Add(node.room);
            foreach (var cell in rect.Cells)
                if (isStartingRoom) debugMap[cell.x, cell.z] = 'S';
                else debugMap[cell.x, cell.z] = 'R';
        }

        public void RoomCellRect(DungeonNode node)
        {
            CellRect result;
            var resultX = Mathf.Clamp(new IntRange(minRoomX, maxRoomX).RandomInRange, minRoomX, node.cells.Width - 1);
            var resultZ = Mathf.Clamp(new IntRange(minRoomZ, maxRoomZ).RandomInRange, minRoomZ, node.cells.Height - 1);
            node.cells.TryFindRandomInnerRect(new IntVec2(resultX, resultZ), out result);
            node.room = result;
            
        }

        public CellRect[] SplitCellRects(CellRect origin)
        {

            //Choose a random direction
            bool splitVertical = (new IntRange(0, 100).RandomInRange) > 50 ? true : false;
            if (origin.Width > origin.Height)
                splitVertical = true;
            if (origin.Height > origin.Width)
                splitVertical = false;
            //Choose a random splitting point
            int splitPoint = splitVertical ?
                Rand.Range((int)(origin.Width * 0.2f), (int)(origin.Width * 0.8f)) :
                Rand.Range((int)(origin.Height * 0.2f), (int)(origin.Height * 0.8f));

            //Split the map in half...
            CellRect dungeonA;
            CellRect dungeonB;
            if (splitVertical)
            {
                dungeonA = new CellRect(origin.minX, origin.minZ, splitPoint - 1, origin.Height);
                dungeonB = new CellRect(origin.minX + splitPoint, origin.minZ, origin.Width - splitPoint, origin.Height);
            }
            else
            {
                dungeonA = new CellRect(origin.minX, origin.minZ, origin.Width, splitPoint - 1);
                dungeonB = new CellRect(origin.minX, origin.minZ + splitPoint, origin.Width, origin.Height - splitPoint);
            }
            //Log.Message($"Split vertical: {splitVertical.ToString()}");
            //Log.Message($"DungeonA Created: {dungeonA.minX} {dungeonA.minZ} {dungeonA.Width} {dungeonA.Height}");
            //Log.Message($"DungeonB Created: {dungeonB.minX} {dungeonB.minZ} {dungeonB.Width} {dungeonB.Height}");
            return new CellRect[] { dungeonA, dungeonB };
        }

    }

}
