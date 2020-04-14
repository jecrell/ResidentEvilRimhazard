using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RERimhazard
{
    public class BuildingToSpawn
    {
        public bool isCenterpiece;
        public ThingDef buildingDef;
        public ThingDef chairDef;
        public ThingDef stuffDef;
        public IntRange numToSpawn;
    }

    public class KindToSpawn
    {
        public PawnKindDef pawnKindDef;
        public IntRange numToSpawn;
        public FactionDef factionDef;
        public bool spawnDead;
    }

    public class RoomFurnishingsDef : Def
    {
        public List<BuildingToSpawn> tinyRoomBuildingsToSpawn; //5 tiles or less
        public List<BuildingToSpawn> smallRoomBuildingsToSpawn; //8 tiles or less
        public List<BuildingToSpawn> mediumRoomBuildingsToSpawn; //12 tiles or less
        public List<BuildingToSpawn> largeRoomBuildingsToSpawn; //24 tiles or less
        public List<BuildingToSpawn> colossalRoomBuildingsToSpawn; //Biggest class
    }

    public class RoomGenDef : Def
    {
        public int maxNumOfRoom = 99;
        public RoomFurnishingsDef furnishingsDef;
        public List<KindToSpawn> pawnsToSpawn;
    }
}
