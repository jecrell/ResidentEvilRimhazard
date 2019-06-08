using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    /// <summary>
    /// Originally ZombePawn from JustinC
    /// </summary>
    public class Zombie : Pawn
    {
        public bool installedBrainChip = false;

        public bool setZombie = false;

        public bool isRaiding = true;

        public bool wasColonist;

        public float notRaidingAttackRange = 15f;

        private bool hadTransformationChance = false;

        private int intervalUntilTransformation = -1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.wasColonist, "wasColonist", false, false);
            Scribe_Values.Look<bool>(ref this.installedBrainChip, "installedBrainChip", false, false);
            Scribe_Values.Look<bool>(ref this.hadTransformationChance, "hadTransformationChance", false);
            //if (Scribe.mode == LoadSaveMode.LoadingVars)
            //{
            //    Cthulhu.Utility.GiveZombieSkinEffect(this);
            //}
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            base.Kill(dinfo, exactCulprit);
            if (this.kindDef.defName != "RE_LickerKind" &&
                this.kindDef.defName != "RE_CrimsonHeadKind")
            {
                //Log.Message($"Added zombie, {this.Label}, to resurrection list");
                this.MapHeld.GetComponent<MapComponent_ZombieTracker>().Notify_ZombieDied(this);
            }
        }
        


        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            this.health.PreApplyDamage(dinfo, out absorbed);
            if (!base.Destroyed && (dinfo.Def == DamageDefOf.Cut || dinfo.Def == DamageDefOf.Stab))
            {
                float num = 0f;
                float num2 = 0f;
                if (dinfo.Instigator != null && dinfo.Instigator is Pawn)
                {
                    Pawn pawn = dinfo.Instigator as Pawn;
                    if (pawn.skills != null)
                    {
                        SkillRecord expr_9B = pawn.skills.GetSkill(SkillDefOf.Melee);
                        num = (float)(expr_9B.Level * 2);
                        num2 = (float)expr_9B.Level / 20f * 3f;
                    }
                    if (UnityEngine.Random.Range(0f, 100f) < 20f + num)
                    {
                        dinfo.SetAmount(999);
                        dinfo.SetHitPart(this.health.hediffSet.GetBrain());
                        dinfo.Def.Worker.Apply(dinfo, this);
                        return;
                    }
                    dinfo.SetAmount((int)((float)dinfo.Amount * (1f + num2)));
                }
            }
        }


        public override void Tick()
        {
            try
            {
                if (DebugSettings.noAnimals && base.RaceProps.Animal)
                {
                    this.Destroy(0);
                }
                if (this.needs != null && this.needs.mood != null)
                {
                    this.needs.mood = null;
                }
                else if (!base.Downed)
                {
                    if (Find.TickManager.TicksGame % 250 == 0)
                    {
                        this.TickRare();
                    }
                    if (base.Spawned)
                    {
                        this.pather.PatherTick();
                        rotationTracker.RotationTrackerTick();
                    }
                    base.Drawer.DrawTrackerTick();
                    this.health.HealthTick();
                    this.records.RecordsTick();
                    if (base.Spawned)
                    {
                        this.stances.StanceTrackerTick();
                        this.verbTracker.VerbsTick();
                        this.natives.NativeVerbsTick();
                    }
                    if (this.equipment != null)
                        this.equipment.EquipmentTrackerTick();

                    if (this.apparel != null)
                        this.apparel.ApparelTrackerTick();

                    if (base.Spawned)
                    {
                        this.jobs.JobTrackerTick();
                    }
                    if (!base.Dead)
                    {
                        this.carryTracker.CarryHandsTick();
                    }
                    if (this.skills != null)
                    {
                        this.skills.SkillsTick();
                    }
                    if (this.inventory != null)
                    {
                        this.inventory.InventoryTrackerTick();
                    }
                }
                if (this.needs != null && this.needs.food != null && this.needs.food.CurLevel <= 0.95f)
                {
                    this.needs.food.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.joy != null && this.needs.joy.CurLevel <= 0.95f)
                {
                    this.needs.joy.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.beauty != null && this.needs.beauty.CurLevel <= 0.95f)
                {
                    this.needs.beauty.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.comfort != null && this.needs.comfort.CurLevel <= 0.95f)
                {
                    this.needs.comfort.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.rest != null && this.needs.rest.CurLevel <= 0.95f)
                {
                    this.needs.rest.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.mood != null && this.needs.mood.CurLevel <= 0.45f)
                {
                    this.needs.mood.CurLevel = 0.5f;
                }
                if (!this.setZombie)
                {
                    this.mindState.mentalStateHandler.neverFleeIndividual = true;
                    this.setZombie = ZombieUtility.Zombify(this);
                    //ZombieMod_Utility.SetZombieName(this);
                }
                if (base.Downed || this.health.Downed || this.health.InPainShock)
                {
                    if (intervalUntilTransformation == -1)
                    {
                        intervalUntilTransformation = Find.TickManager.TicksGame + new IntRange(RESettings.DOWNZOMBIE_TRANSFORM_INTERVAL_MIN, RESettings.DOWNZOMBIE_TRANSFORM_INTERVAL_MAX).RandomInRange;
                    }
                    if (Find.TickManager.TicksGame > intervalUntilTransformation)
                    {
                        if (ResolveTransformations()) return;
                    }
                    //DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, 9999, 1f, -1f, this, null, null);
                    //damageInfo.SetHitPart(this.health.hediffSet.GetBrain());
                    //damageInfo.SetPart(new BodyPartDamageInfo(this.health.hediffSet.GetBrain(), false, HediffDefOf.Cut));
                    //base.TakeDamage(damageInfo);
                }
            }
            catch (Exception)
            {
            }
        }

        private bool ResolveTransformations()
        {
            if (!hadTransformationChance)
            {
                hadTransformationChance = true;
                if (Rand.Value <= RESettings.MUTATION_CHANCE)
                {
                    var curLoc = this.PositionHeld;
                    var curMap = this.MapHeld;
                    var zFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("RE_Zombies"));

                    HediffDef zHDef;
                    PawnKindDef zKind = ResolveTransformationKind(out zHDef);

                    Pawn newThing;
                    if (zKind.defName != "RE_CrimsonHeadKind")
                    {
                        this.Destroy();
                        FilthMaker.MakeFilth(curLoc, curMap, ThingDefOf.Filth_Blood, Rand.Range(5, 8));
                        newThing = PawnGenerator.GeneratePawn(zKind, zFaction);

                    }
                    else
                    {
                        //The function here actually destroys the zombie
                        var facName = this?.Faction?.def?.defName ?? "RE_Zombies";
                        newThing = ZombieUtility.CreateZombieAtSourcePawnLocation(this, "RE_CrimsonHeadKind", facName);
                    }

                    HealthUtility.AdjustSeverity(newThing, zHDef, 1.0f);
                        GenSpawn.Spawn(newThing, curLoc, curMap);

                    ((Zombie)newThing).hadTransformationChance = true;
                    return true;
                }
            }
            return false;
        }

        private PawnKindDef ResolveTransformationKind(out HediffDef zHDef)
        {
            var zombieInfo = ZombieUtility.GetPawnKindDefForRandomResurrectedZombie();
            zHDef = zombieInfo.zHediff;
            return zombieInfo.zKind;
        }
    }
}
