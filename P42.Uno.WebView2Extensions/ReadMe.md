# P42.Uno.WebView2Extensions

## Release Build Issue

When building a project that uses P42.Uno.WebView2Extensions as a Project Reference, the build will fail because /Users/ben/Development/AWC/ConnectionCalc.UNO/Bc3/P42.Uno.HtmlExtensions/P42.Uno.WebView2Extensions/bin/Release/net10.0-android/P42.Uno.WebView2Extensions.aar will have zero bytes.  This is not the case when building P42.Uno.WebView2Extensions.

The empty .aar comes from Rider's NuGet Pack step, not from a race. My earlier race theory was wrong, and the solution platform fix doesn't address this, though it does no harm. I reproduced it from the command line and confirmed it in the build logs.

What happens:

  1. `P42_LocalNuGetFolderPath` has been set before build (I don't remember where!), so `P42.Uno.WebView2Extensions.NugetPackage.props` turns on `GeneratePackageOnBuild`. Every Rider Release build therefore runs Build and then a separate Pack. 
  2. Build creates a good `.aar` (514 bytes): `res/xml/p42_uno_webview2_network_security_config.xml` plus `.net/__res_name_case_map.txt`.
  3. Pack runs in a fresh MSBuild project instance, outside the one that did the build. It calls `_IncludeAarInNuGetPackage`, which depends on `_CreateAar`. In that instance the Android resource paths (`@(_AndroidResourceDest)`) were never computed, so `_CreateAar` gets no resources.
  4. With the resources missing, the input hash changes, so `obj/.../P42.Uno.WebView2Extensions.aar.cache` gets rewritten. It is now newer than the `.aar`, so `_CreateAar` reruns.
  5. The only remaining input is `Platforms/Android/AndroidManifest.xml`, and it's skipped because it has no package attribute. `CreateAar` ends up with zero entries. In the installed SDK (36.1.118, which uses libZipSharp) that writes a 0‑byte file. I confirmed this by running the task by itself with no inputs.
  6. The next project to build (MarkdownExtensions, HtmlExtensions or Bc3.Forms) opens that file and fails with XA1004. 

  Fix options:                                                                                                                                                          
  - Recommended, in P42.Uno.WebView2Extensions.csproj: make Pack compute the resource paths before it rebuilds the .aar:                                                
  <Target Name="P42_ComputeAarInputs"                     
          Condition="$(TargetFramework.Contains('android'))"
          BeforeTargets="_CreateAarCache"                             
          DependsOnTargets="_ComputeAndroidResourcePaths" />
  - Or stop Rider from setting `P42_LocalNuGetFolderPath`, and pack only from the mac* scripts.
                                                                                                                                                                        
  Separately, the network security config is listed twice in AndroidResource: once explicitly in the csproj and once picked up automatically from Platforms/Android/Resources. It's harmless, but you can remove the explicit <AndroidResource Include=…>.  