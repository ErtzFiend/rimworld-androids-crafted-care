using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AndroidsCraftedCare
{
    /// <summary>
    /// Bootstrap and classifier. Android patients are identified only by exact names:
    /// a DefModExtension of type Androids.MechanicalPawnProperties, this mod's own
    /// CraftedCareMechanicalProperties extension, or the ChjAndroidLike hediff.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class CraftedCareMain
    {
        public const string HarmonyId = "jacob.androidscraftedcare";

        static CraftedCareMain()
        {
            new Harmony(HarmonyId).PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool IsCraftedAndroid(Pawn pawn)
        {
            if (pawn == null || pawn.def == null) return false;

            var extensions = pawn.def.modExtensions ?? new List<DefModExtension>();
            foreach (var ext in extensions)
            {
                if (ext == null) continue;
                if (ext is CraftedCareMechanicalProperties) return true;
                if (ext.GetType().FullName == "Androids.MechanicalPawnProperties") return true;
            }

            if (pawn.health != null && pawn.health.hediffSet != null)
                foreach (var h in pawn.health.hediffSet.hediffs)
                    if (h?.def != null && h.def.defName == "ChjAndroidLike") return true;

            return false;
        }

        /// <summary>
        /// True when a doctor pawn is performing a vanilla TendPatient job whose target is a
        /// crafted android. Used to scope the Crafted Care behaviour to androids only.
        /// </summary>
        public static bool IsTendingCraftedAndroid(Pawn doctor, Pawn patient)
        {
            if (patient == null || !IsCraftedAndroid(patient)) return false;
            var job = doctor?.jobs?.curJob;
            return job != null && job.def == JobDefOf.TendPatient;
        }
    }
}
