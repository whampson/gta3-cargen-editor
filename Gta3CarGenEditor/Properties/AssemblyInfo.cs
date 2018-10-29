using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: AssemblyTitle("GTA3 Parked Car Editor")]
[assembly: AssemblyDescription("A simple tool for editing parked car generators in a GTA3 savegame.")]
[assembly: AssemblyProduct("GTA3 Parked Car Editor")]
[assembly: AssemblyCopyright("Copyright (C) 2018 W. Hampson. All rights reserved.")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]

/**
 * In order to begin building localizable applications, set
 * <UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
 * inside a <PropertyGroup>.  For example, if you are using US English
 * in your source files, set the <UICulture> to en-US.  Then uncomment
 * the NeutralResourceLanguage attribute below.  Update the "en-US" in
 * the line below to match the UICulture setting in the project file.
 */
//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            // theme-specific resource dictionaries
    ResourceDictionaryLocation.SourceAssembly   // generic resource dictionary 
)]

[assembly: AssemblyVersion("1.1.0.528")]
[assembly: AssemblyFileVersion("1.1.0.528")]
[assembly: AssemblyInformationalVersion("1.1.0")]
