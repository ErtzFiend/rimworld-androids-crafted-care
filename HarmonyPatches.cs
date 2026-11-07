using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AndroidsCraftedCare
{
    /// <summary>
    /// Keeps vanilla doctors from tending crafted androids. The Crafting work giver is the
    /// only path that tends them. Organic patients are untouched.
    /// </summary>
    [HarmonyPatch(typeof(WorkGiver_Tend), nameof(WorkGiver_Tend.HasJobOnThing))]
    public static class Patch_WorkGiverTend_HasJobOnThing
    {
        public static bool Prefix(object __instance, Pawn pawn, Thing t, ref bool __result)
        {
            if (__instance is WorkGiver_TendAndroid) return true;
            if (t is Pawn patient && CraftedCareMain.IsCraftedAndroid(patient))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Tending SPEED for a crafted android follows the doctor's Crafting skill instead of Medicine.
    /// Vanilla computes the tend wait as (1 / MedicalTendSpeed) * 600, so scaling that stat by
    /// (craftingFactor / medicineFactor) makes the wait Crafting-derived. The factors come from the
    /// installed StatDef's own skill-need curve, so no constant is invented. Organic tending is untouched.
    /// </summary>
    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue),
        new[] { typeof(Thing), typeof(StatDef), typeof(bool), typeof(int) })]
    public static class Patch_StatExtension_GetStatValue
    {
        static readonly AccessTools.FieldRef<SkillNeed_BaseBonus, float> BaseRef =
            AccessTools.FieldRefAccess<SkillNeed_BaseBonus, float>("baseValue");
        static readonly AccessTools.FieldRef<SkillNeed_BaseBonus, float> PerLevelRef =
            AccessTools.FieldRefAccess<SkillNeed_BaseBonus, float>("bonusPerLevel");

        public static void Postfix(Thing thing, StatDef stat, ref float __result)
        {
            if (stat != StatDefOf.MedicalTendSpeed || !(thing is Pawn doctor)) return;

            var patient = doctor.jobs?.curJob?.targetA.Thing as Pawn;
            if (!CraftedCareMain.IsTendingCraftedAndroid(doctor, patient)) return;

            var needs = StatDefOf.MedicalTendSpeed.skillNeedFactors;
            if (needs == null) return;

            foreach (var need in needs)
            {
                if (need == null || need.skill != SkillDefOf.Medicine) continue;
                if (!(need is SkillNeed_BaseBonus curve)) return;

                float medicine = BaseRef(curve) + PerLevelRef(curve) * doctor.skills.GetSkill(SkillDefOf.Medicine).Level;
                float crafting = BaseRef(curve) + PerLevelRef(curve) * doctor.skills.GetSkill(SkillDefOf.Crafting).Level;
                if (medicine <= 0f) return;

                __result *= crafting / medicine;
                return;
            }
        }
    }

    /// <summary>
    /// The job's DISPLAYED skill follows Crafting. Vanilla JobDriver_TendPatient attaches
    /// waitToil.activeSkill = () => SkillDefOf.Medicine, so every reader of
    /// JobDriver.ActiveSkill (passionate-work thoughts, job/skill displays) reported Medicine
    /// while a colonist tended an android. This postfix rewrites that report to Crafting,
    /// only for TendPatient jobs whose target is a crafted android. Organic tending is untouched.
    /// </summary>
    [HarmonyPatch(typeof(JobDriver), "get_ActiveSkill")]
    public static class Patch_JobDriver_ActiveSkill
    {
        public static void Postfix(JobDriver __instance, ref SkillDef __result)
        {
            if (__result != SkillDefOf.Medicine) return;
            if (__instance?.job == null || __instance.job.def != JobDefOf.TendPatient) return;

            var patient = __instance.job.targetA.Thing as Pawn;
            if (!CraftedCareMain.IsCraftedAndroid(patient)) return;

            __result = SkillDefOf.Crafting;
        }
    }
}
