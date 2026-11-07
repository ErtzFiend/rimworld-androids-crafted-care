# Androids: Crafted Care

RimWorld 1.7 mod for [ChJees Androids](https://steamcommunity.com/sharedfiles/filedetails/?id=2889326293).
Tending an android is a **Crafting** job instead of a Medical one. Androids still go to medical beds and are
still repaired with the Androids mod's own repair kits/parts.

## What this mod changes
1. **Job assignment** - a new work giver (`WorkGiver_TendAndroid`, Crafting work type) derives from vanilla
   `WorkGiver_Tend`. A colonist takes an android tending job under the **Crafting** work type, and only when
   that colonist has Crafting work enabled.
2. **Medical work type no longer tends androids** - a Harmony prefix on `WorkGiver_Tend.HasJobOnThing` stops the
   vanilla doctor/Medical work giver from picking android patients. Organic patients are untouched.
3. **Tend speed follows Crafting** - tending speed is the `MedicalTendSpeed` stat. While a colonist is on a
   `TendPatient` job for an android, that stat is scaled by (Crafting skill factor / Medicine skill factor),
   using the curve from the installed stat def. No invented constants.

## What this mod does NOT change (verified against the Androids assembly)
- **Repair materials.** The Androids mod itself patches `HealthAIUtility.FindBestMedicine` so android patients
  get the nearest item whose def carries `Androids.DroidRepairProperties` - its repair kits/parts, **not vanilla
  medicine**. This mod leaves that path alone.
- **Crafting XP.** The Androids mod's own `Toils_Tend.FinalizeTend` replacement already awards **Crafting** XP
  for android tending (`Learn(SkillDefOf.Crafting, 500 x factor)`). No XP patch is needed here.
- **Medical beds and medicine consumption** for organic colonists.

## Android detection
Exact matches only, no substring matching: a `DefModExtension` whose type is `Androids.MechanicalPawnProperties`,
this mod's own `CraftedCareMechanicalProperties` extension, or a hediff whose defName is exactly `ChjAndroidLike`
(Android Conversion converts organics in place, so the hediff matters).

## Known gaps
- The job's displayed skill comes from the vanilla driver and still reads as Medicine; only the speed stat and
  the work-type assignment are redirected.
- Tending **quality** is not a Medical lever for androids: the Androids repair path either fully repairs the
  hediff when a repair kit is present, or applies a fixed 0.1 tend when there is none.
- **Not tested in a live game.** The regression tests cover patch registration and gating logic only.

## Build and test
```
dotnet build AndroidsCraftedCare.csproj -c Release -p:GameManagedDir=/path/to/RimWorldWin64_Data/Managed
dotnet run --project tests/AndroidsCraftedCare.Tests.csproj -c Release
```
`GameManagedDir` defaults to `/root/rimworld_libs`. Dependencies: Harmony, ChJees.Androids14.
