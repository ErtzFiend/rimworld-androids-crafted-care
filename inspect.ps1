$gameRoot = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld"
$mods = $gameRoot + "\Mods"
if ([System.IO.Path]::Exists($mods)) {
  Get-ChildItem -Path $mods -Directory -Force | ForEach-Object {
    $about = $_.FullName + "\About\About.xml"
    if ([System.IO.Path]::Exists($about)) {
      Write-Output ("=== " + $_.Name + " ===")
      Write-Output (Get-Content -Path $about -Raw)
    }
  }
} else { Write-Output ("MODSDIR MISSING") }
