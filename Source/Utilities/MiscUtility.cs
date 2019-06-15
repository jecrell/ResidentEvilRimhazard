using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RERimhazard
{
    public static class MiscUtility
    {
        static public void SpawnHerbsAtRect(CellRect rect, Map map, int minRange = 0, int maxRange = 3)
        {
            var randomRange = new IntRange(minRange, maxRange).RandomInRange;
            for (int i = 0; i < randomRange; i++)
            {
                ThingDef thingDef =
                    Rand.Value > 0.5f ? ThingDef.Named("RE_Plant_ResidentEvilHerbGreen") :
                        Rand.Value > 0.5f ? ThingDef.Named("RE_Plant_ResidentEvilHerbRed") :
                                            ThingDef.Named("RE_Plant_ResidentEvilHerbBlue");
                int age = thingDef.plant.LimitedLifespan ? Rand.Range(0, Mathf.Max(thingDef.plant.LifespanTicks - 2500, 0)) : 0;
                Plant plant = (Plant)GenSpawn.Spawn(thingDef, rect.RandomCell, map);
                plant.Growth = 1f;
                if (plant.def.plant.LimitedLifespan)
                {
                    plant.Age = age;
                }
            }
        }
        static public void GenerateRandomAge(Pawn pawn, Map map)
        {
            int num = 0;
            int num2;
            do
            {
                if (pawn.RaceProps.ageGenerationCurve != null)
                {
                    num2 = Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve));
                }
                else if (pawn.RaceProps.IsMechanoid)
                {
                    num2 = Rand.Range(0, 2500);
                }
                else
                {
                    if (!pawn.RaceProps.Animal)
                    {
                        goto IL_84;
                    }
                    num2 = Rand.Range(1, 10);
                }
                num++;
                if (num > 100)
                {
                    goto IL_95;
                }
            }
            while (num2 > pawn.kindDef.maxGenerationAge || num2 < pawn.kindDef.minGenerationAge);
            goto IL_A5;
        IL_84:
            Log.Warning("Didn't get age for " + pawn);
            return;
        IL_95:
            Log.Error("Tried 100 times to generate age for " + pawn);
        IL_A5:
            pawn.ageTracker.AgeBiologicalTicks = ((long)(num2 * 3600000f) + Rand.Range(0, 3600000));
            int num3;
            if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
            {
                float value = Rand.Value;
                if (value < 0.7f)
                {
                    num3 = Rand.Range(0, 100);
                }
                else if (value < 0.95f)
                {
                    num3 = Rand.Range(100, 1000);
                }
                else
                {
                    int num4 = GenLocalDate.Year(map) - 2026 - pawn.ageTracker.AgeBiologicalYears;
                    num3 = Rand.Range(1000, num4);
                }
            }
            else
            {
                num3 = 0;
            }
            long num5 = GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;
            num5 -= num3 * 3600000L;
            pawn.ageTracker.BirthAbsTicks = num5;
            if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
            {
                pawn.ageTracker.AgeChronologicalTicks = (pawn.ageTracker.AgeBiologicalTicks);
            }
        }
    }
}
