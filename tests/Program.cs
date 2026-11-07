using System;
using System.Linq;
using System.Runtime.Loader;
using AndroidsCraftedCare;
using HarmonyLib;
using RimWorld;
using Verse;

internal static class Program
{
    private static int failures;

    private static void Check(bool ok, string name)
    {
        Console.WriteLine((ok ? "PASS " : "FAIL ") + name);
        if (!ok) failures++;
    }

    private static int Main()
    {
        // The RimWorld facades request Microsoft.Extensions.* at 11.0; the 10.x runtime copies satisfy them.
        AssemblyLoadContext.Default.Resolving += (ctx, name) =>
        {
            var path = "/root/.nuget/packages/microsoft.aspnetcore.app.runtime.linux-x64/10.0.14/runtimes/linux-x64/lib/net10.0/" + name.Name + ".dll";
            return System.IO.File.Exists(path) ? ctx.LoadFromAssemblyPath(path) : null;
        };

        // Game startup runs the StaticConstructorOnStartup hook, which applies the Harmony patches.
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CraftedCareMain).TypeHandle);

        // 1. Crafting-path work giver derives from the vanilla tend work giver.
        Check(typeof(WorkGiver_TendAndroid).BaseType == typeof(WorkGiver_Tend),
              "WorkGiver_TendAndroid derives from WorkGiver_Tend");

        // 2. The Harmony prefix is registered against the vanilla HasJobOnThing.
        var target = AccessTools.Method(typeof(WorkGiver_Tend), nameof(WorkGiver_Tend.HasJobOnThing));
        var info = Harmony.GetPatchInfo(target);
        bool prefixRegistered = info != null && info.Prefixes.Any(p =>
            p.PatchMethod.DeclaringType == typeof(Patch_WorkGiverTend_HasJobOnThing) && p.PatchMethod.Name == "Prefix");
        Check(prefixRegistered, "Harmony prefix is registered on WorkGiver_Tend.HasJobOnThing");

        // 3. Prefix lets WorkGiver_TendAndroid run the original check (not blocked).
        bool result = true;
        bool runOriginal = Patch_WorkGiverTend_HasJobOnThing.Prefix(new WorkGiver_TendAndroid(), null, null, ref result);
        Check(runOriginal, "prefix does not block WorkGiver_TendAndroid");

        // 4. Prefix does not block a non-android target for the base doctor work giver.
        bool result2 = true;
        bool runOriginal2 = Patch_WorkGiverTend_HasJobOnThing.Prefix(new WorkGiver_Tend(), null, null, ref result2);
        Check(runOriginal2 && result2, "prefix leaves a null target to vanilla");

        // 5. Classifier: null is never an android.
        Check(!CraftedCareMain.IsCraftedAndroid(null), "IsCraftedAndroid(null) is false");

        // 6. Crafting-based tend speed patch is registered on the real GetStatValue overload.
        var gsv = AccessTools.Method(typeof(StatExtension), "GetStatValue",
            new[] { typeof(Thing), typeof(StatDef), typeof(bool), typeof(int) });
        var gsvInfo = Harmony.GetPatchInfo(gsv);
        bool gsvRegistered = gsvInfo != null && gsvInfo.Postfixes.Any(p =>
            p.PatchMethod.DeclaringType == typeof(Patch_StatExtension_GetStatValue));
        Check(gsv != null, "StatExtension.GetStatValue overload resolves");
        Check(gsvRegistered, "speed postfix is registered on StatExtension.GetStatValue");

        Console.WriteLine(failures == 0 ? "ALL TESTS PASSED" : failures + " TEST(S) FAILED");
        return failures == 0 ? 0 : 1;
    }
}
