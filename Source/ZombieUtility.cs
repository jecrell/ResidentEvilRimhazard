using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RERimhazard
{
    public static class ZombieUtility
    {
        public static bool Zombify(Zombie pawn)
        {
            if (pawn.Drawer == null)
            {
                return false;
            }
            if (pawn.Drawer.renderer == null)
            {
                return false;
            }
            if (pawn.Drawer.renderer.graphics == null)
            {
                return false;
            }
            if (!pawn.Drawer.renderer.graphics.AllResolved)
            {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
            if (pawn.Drawer.renderer.graphics.headGraphic == null)
            {
                return false;
            }
            if (pawn.Drawer.renderer.graphics.nakedGraphic == null)
            {
                return false;
            }
            if (pawn.Drawer.renderer.graphics.headGraphic.path == null)
            {
                return false;
            }
            if (pawn.Drawer.renderer.graphics.nakedGraphic.path == null)
            {
                return false;
            }
            GiveZombieSkinEffect(pawn);
            return true;
        }

        public static void GiveZombieSkinEffect(Zombie pawn, Pawn sourcePawn = null)
        {
            if (sourcePawn == null) sourcePawn = pawn;
            Color newSkin = RESettings.SKINZOMBIE;

            Graphic nakedBodyGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, newSkin);
            Graphic headGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.HeadGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, newSkin);
            Graphic hairGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, sourcePawn.story.hairColor);
            pawn.Drawer.renderer.graphics.headGraphic = headGraphic;
            pawn.Drawer.renderer.graphics.nakedGraphic = nakedBodyGraphic;
            pawn.Drawer.renderer.graphics.hairGraphic = hairGraphic;
        }


        // Credit goes to Justin C for the Zombie Apocalypse code.
        // Taken from Verse.ZombieMod_Utility
        public static Pawn GenerateZombiePawnFromSource(Pawn sourcePawn)
        {
            PawnKindDef pawnKindDef = PawnKindDef.Named("RE_ZombieKind");
            Faction factionDirect = Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile);
            Pawn pawn = (Pawn)ThingMaker.MakeThing(pawnKindDef.race, null);
            pawn.kindDef = pawnKindDef;
            pawn.SetFactionDirect(factionDirect);
            pawn.pather = new Pawn_PathFollower(pawn);
            pawn.ageTracker = new Pawn_AgeTracker(pawn);
            pawn.health = new Pawn_HealthTracker(pawn);
            pawn.jobs = new Pawn_JobTracker(pawn);
            pawn.mindState = new Pawn_MindState(pawn);
            pawn.filth = new Pawn_FilthTracker(pawn);
            pawn.needs = new Pawn_NeedsTracker(pawn);
            pawn.stances = new Pawn_StanceTracker(pawn);
            pawn.natives = new Pawn_NativeVerbs(pawn);
            PawnComponentsUtility.CreateInitialComponents(pawn);
            if (pawn.RaceProps.ToolUser)
            {
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
                pawn.carryTracker = new Pawn_CarryTracker(pawn);
                pawn.apparel = new Pawn_ApparelTracker(pawn);
                pawn.inventory = new Pawn_InventoryTracker(pawn);
            }
            if (pawn.RaceProps.Humanlike)
            {
                pawn.ownership = new Pawn_Ownership(pawn);
                pawn.skills = new Pawn_SkillTracker(pawn);
                pawn.relations = new Pawn_RelationsTracker(pawn);
                pawn.story = new Pawn_StoryTracker(pawn);
                pawn.workSettings = new Pawn_WorkSettings(pawn);
            }
            if (pawn.RaceProps.intelligence <= Intelligence.ToolUser)
            {
                pawn.caller = new Pawn_CallTracker(pawn);
            }
            //pawn.gender = Gender.None;
            pawn.gender = sourcePawn.gender;
            MiscUtility.GenerateRandomAge(pawn, pawn.Map);
            pawn.needs.SetInitialLevels();
            if (pawn.RaceProps.Humanlike)
            {
                string headGraphicPath = sourcePawn.story.HeadGraphicPath;
                typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, headGraphicPath);
                pawn.story.melanin = sourcePawn.story.melanin;
                pawn.story.crownType = sourcePawn.story.crownType;
                pawn.story.hairColor = sourcePawn.story.hairColor;
                NameTriple name = sourcePawn.Name as NameTriple;
                pawn.Name = name;
                pawn.story.childhood = sourcePawn.story.childhood;
                pawn.story.adulthood = sourcePawn.story.adulthood;
                pawn.story.hairDef = sourcePawn.story.hairDef;
                foreach (Trait current in sourcePawn.story.traits.allTraits)
                {
                    pawn.story.traits.GainTrait(current);
                }
                //pawn.story.GenerateSkillsFromBackstory();
            }
            GenerateZombieApparelFromSource(pawn, sourcePawn);
            PawnGenerationRequest con = new PawnGenerationRequest();
            PawnInventoryGenerator.GenerateInventoryFor(pawn, con);
            //Graphic nakedBodyGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(sourcePawn.story.bodyType, ShaderDatabase.CutoutSkin, new Color(0.37f, 0.48f, 0.35f, 1f));
            Graphic nakedBodyGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, new Color(0.37f, 0.48f, 0.35f, 1f));
            Graphic headGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.HeadGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, new Color(0.37f, 0.48f, 0.35f, 1f));
            Graphic hairGraphic = GraphicDatabase.Get<Graphic_Multi>(sourcePawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, sourcePawn.story.hairColor);
            pawn.Drawer.renderer.graphics.headGraphic = headGraphic;
            pawn.Drawer.renderer.graphics.nakedGraphic = nakedBodyGraphic;
            pawn.Drawer.renderer.graphics.hairGraphic = hairGraphic;
            return pawn;
        }

        public static void CreateZombieAtSourcePawnLocation(Pawn source)
        {
            Zombie newPawn = DoGenerateZombiePawnFromSource(source);
            var sourcePos = source.PositionHeld;
            var sourceMap = source.MapHeld;
            if (source?.Corpse is Corpse c)
            c.Destroy();
            if (source != null && !source.Destroyed)
                source.Destroy();
            GenSpawn.Spawn(newPawn, sourcePos, sourceMap);
            
        }


        public static Zombie DoGenerateZombiePawnFromSource(Pawn sourcePawn)
        {
            PawnKindDef pawnKindDef = PawnKindDef.Named("RE_ZombieKind");
            Faction factionDirect = Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile);
            Zombie pawn = (Zombie)ThingMaker.MakeThing(pawnKindDef.race, null);
            try
            {
                pawn.kindDef = pawnKindDef;
                pawn.SetFactionDirect(factionDirect);
                PawnComponentsUtility.CreateInitialComponents(pawn);
                pawn.gender = sourcePawn.gender;
                pawn.ageTracker.AgeBiologicalTicks = sourcePawn.ageTracker.AgeBiologicalTicks;
                pawn.ageTracker.AgeChronologicalTicks = sourcePawn.ageTracker.AgeChronologicalTicks;
                pawn.workSettings = new Pawn_WorkSettings(pawn);
                if (pawn.workSettings != null && sourcePawn.Faction.IsPlayer)
                {
                    pawn.workSettings.EnableAndInitialize();
                }

                pawn.needs.SetInitialLevels();
                //Add hediffs?
                //Add relationships?
                if (pawn.RaceProps.Humanlike)
                {
                    pawn.story.melanin = sourcePawn.story.melanin;
                    pawn.story.crownType = sourcePawn.story.crownType;
                    pawn.story.hairColor = sourcePawn.story.hairColor;
                    pawn.story.childhood = sourcePawn.story.childhood;
                    pawn.story.adulthood = sourcePawn.story.adulthood;
                    pawn.story.bodyType = sourcePawn.story.bodyType;
                    pawn.story.hairDef = sourcePawn.story.hairDef;

                    SkillFixer(pawn, sourcePawn);
                    RelationshipFixer(pawn, sourcePawn);
                    AddedPartFixer(pawn, sourcePawn);
                    //pawn.story.GenerateSkillsFromBackstory();
                    NameTriple nameTriple = sourcePawn.Name as NameTriple;
                    pawn.Name = new NameTriple(nameTriple.First, string.Concat(new string[]
                        {
                        "* ",
                        Translator.Translate("RE_Zombie"),
                        " ",
                        nameTriple.Nick,
                        " *"
                        }), nameTriple.Last);
                }
                string headGraphicPath = sourcePawn.story.HeadGraphicPath;
                typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, headGraphicPath);
                GenerateZombieApparelFromSource(pawn, sourcePawn);
                PawnGenerationRequest con = new PawnGenerationRequest();
                PawnInventoryGenerator.GenerateInventoryFor(pawn, con);
                GiveZombieSkinEffect(pawn, sourcePawn as Zombie);
                //if (isBerserk)
                //{
                //    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                //}
                //Log.Message(pawn.Name.ToStringShort);
                return pawn;
            }
            catch (Exception e)
            {
                //Cthulhu.Utility.DebugReport(e.ToString());
            }
            return null;
        }

        public static void AddedPartFixer(Zombie pawn, Pawn sourcePawn = null)
        {
            foreach (Hediff hediff in sourcePawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_AddedPart || hediff is Hediff_Implant)
                {
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        public static void SkillFixer(Zombie pawn, Pawn sourcePawn = null)
        {
            //Add in and fix skill levels
            foreach (SkillRecord skill in sourcePawn.skills.skills)
            {
                SkillRecord pawnSkill = pawn.skills.GetSkill(skill.def);
                if (pawnSkill == null) pawn.skills.skills.Add(skill);
                else
                {
                    pawnSkill.Level = skill.Level;
                    pawnSkill.passion = skill.passion;
                }
            }
        }

        public static void RelationshipFixer(Zombie pawn, Pawn sourcePawn = null)
        {
            //Add in and fix all blood relationships
            if (sourcePawn.relations.DirectRelations != null && sourcePawn.relations.DirectRelations.Count > 0)
            {
                foreach (DirectPawnRelation pawnRel in sourcePawn.relations.DirectRelations)
                {
                    if (pawnRel.otherPawn != null && pawnRel.def != null)
                    {
                        pawn.relations.AddDirectRelation(pawnRel.def, pawnRel.otherPawn);
                    }
                }
                sourcePawn.relations.ClearAllRelations();
            }
        }


        // More of Justin C's work. I can't take credit for this.
        // Verse.ZombieMod_Utility
        public static void GenerateZombieApparelFromSource(Pawn zombie, Pawn sourcePawn)
        {
            if (sourcePawn.apparel == null || sourcePawn.apparel.WornApparelCount == 0)
            {
                return;
            }
            foreach (Apparel current in sourcePawn.apparel.WornApparel)
            {
                Apparel apparel;
                if (current.def.MadeFromStuff)
                {
                    apparel = (Apparel)ThingMaker.MakeThing(current.def, current.Stuff);
                }
                else
                {
                    apparel = (Apparel)ThingMaker.MakeThing(current.def, null);
                }
                apparel.DrawColor = new Color(current.DrawColor.r, current.DrawColor.g, current.DrawColor.b, current.DrawColor.a);
                zombie.apparel.Wear(apparel, true);
            }
        }



    }
}