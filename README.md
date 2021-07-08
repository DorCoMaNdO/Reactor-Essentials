# Essentials
Essentials is a modding library for Among Us with APIs to speed up and ease development, with the goal of improving mod compatibility.

Currently, the key features of Essentials are custom options and gameplay buttons, with more planned in the future to improve mod compatibility.

## Installation
**NOTE:** At the moment, official builds are only compiled for the Steam client, however builds for 2021.5.10 or newer should work on all platforms.
1. Install [BepInEx](https://docs.reactor.gg/quick_start/install_bepinex).
2. Install [Reactor](https://docs.reactor.gg/quick_start/install_reactor) ([CI 38](https://github.com/NuclearPowered/Reactor/actions/runs/593649307) for Essentials 0.1.1+, [CI 45](https://github.com/NuclearPowered/Reactor/actions/runs/636023321) for Among Us version 2021.3.5s, [CI 47](https://github.com/NuclearPowered/Reactor/actions/runs/723875068) for 2021.3.31.3s, [CI 55](https://github.com/NuclearPowered/Reactor/actions/runs/748060171) for 2021.4.12s, [CI 56](https://github.com/NuclearPowered/Reactor/actions/runs/791867842) for 2021.4.14s, [CI 64](https://github.com/NuclearPowered/Reactor/actions/runs/926109011) for 2021.5.10s, [CI 67 or newer](https://github.com/NuclearPowered/Reactor/actions/runs/940598811) for 2021.6.15s and 2021.6.30s).
3. Grab the [latest release](https://github.com/DorCoMaNdO/Reactor-Essentials/releases/latest) for your client version (support for older clients may be dropped, in that case browse [previous releases](https://github.com/DorCoMaNdO/Reactor-Essentials/releases)).
4. Place the downloaded release in `Among Us/BepInEx/plugins/` (same steps as installing Reactor).

## Development
To develop plugins with Essentials, Essentials needs to be installed, follow the steps above before proceeding.
Please configure notifications for future releases (`Watch` -> `Custom` -> `Releases`) to keep your projects up to date with bug fixes and new features.
1. Open your project file (`.csproj`).
2. Add or locate an `ItemGroup` tag.
3. Add the appropriate line 
* When using an obfuscated version (2021.4.14s or older, when using Reactor.OxygenFilter.MSBuild): `<Deobfuscate Include="$(AmongUs)\BepInEx\plugins\Essentials-$(GameVersion).dll" />`
* For any other version: `<Reference Include="$(AmongUs)\BepInEx\plugins\Essentials-$(GameVersion).dll" />`
4. Build. Note that if you are using Visual Studio and modding an obfuscated version, building your project once with `dotnet build` may be required due to a Mono.Cecil issue present in Reactor.OxygenFilter.MSBuild.
5. While a guide is not currently available, there's a demo project "Convert", also most types and methods contain documentation, the current key features are under the `Essentials.Options` and `Essentials.UI` namespaces.

## Building Essentials
Newer versions of Essentials use configurations based on Among Us target version(s) and override the `AmongUs` environment variable as a result.
Essentials depends on Reactor, follow installation steps 1 and 2 before proceeding.

### Building for a single Among Us version
1. [Set up the `AmongUs` environment variable](https://docs.reactor.gg/quick_start/install_netsdk_template#setup-among-us-environment-variable)
2. Select and build the project configuration targeting your target version, building with `dotnet build -c CONFIGURATION` (where `CONFIGURATION` is the configuration name, ex: `S20210412`), may be required when building for an obfuscated version due to a Mono.Cecil issue present in Reactor.OxygenFilter.MSBuild.
3. The compiled binary will be copied to the `plugins` folder of your targeted Among Us version, as well as a `bin` folder in the solution's folder.

### Building for multiple Among Us versions
1. Add an environment variable named `AmongUsMods` targeting the parent folder that contains different Among Us versions in sub-folders, for example: `%AmongUsMods%\2021.4.12s` should lead to the `2021.4.12s` sub-folder.
2. Edit the `build all.bat` script (in the root folder of the solution) so that it would only contain the Among Us versions you're targeting.
3. Run `build all.bat` (alternatively, in Visual Studio you can use `Build` -> `Batch Build...` from the toolbar and select all the target versions and then `Build`, running `build all.bat` may still be required when Reactor is updated).
4. The compiled binary will be copied to the `plugins` folder of your targeted Among Us versions, as well as a `bin` folder in the solution's folder.

*This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.*