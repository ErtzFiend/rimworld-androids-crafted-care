using RimWorld;
using Verse;
using Verse.AI;

namespace AndroidsCraftedCare
{
    /// <summary>
    /// Crafting-work giver that sends a worker to tend a crafted android in a medical bed.
    /// It inherits vanilla WorkGiver_Tend job creation, so medicine, bed and reservation
    /// behaviour are unchanged. Only the eligibility gate changes: the worker must have
    /// Crafting work enabled and the patient must be a crafted android.
    /// </summary>
    public class WorkGiver_TendAndroid : WorkGiver_Tend
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn patient) || !CraftedCareMain.IsCraftedAndroid(patient)) return false;
            if (pawn.workSettings == null || !pawn.workSettings.WorkIsActive(WorkTypeDefOf.Crafting)) return false;
            return base.HasJobOnThing(pawn, t, forced);
        }
    }
}
