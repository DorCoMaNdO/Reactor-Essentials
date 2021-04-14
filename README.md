# Essentials
Essentials is a modding library for Among Us with APIs to speed up and ease development, with the goal of improving mod compatibility.

## Installation
**NOTE:** At the moment, official builds are only compiled for the Steam client.
1. Install [BepInEx](https://docs.reactor.gg/docs/basic/install_bepinex).
2. Install [Reactor](https://docs.reactor.gg/docs/basic/install_reactor) ([CI 38 or newer](https://github.com/NuclearPowered/Reactor/actions/runs/593649307) for Essentials 0.1.1+, [CI 45](https://github.com/NuclearPowered/Reactor/actions/runs/636023321) for Among Us version 2021.3.5s, [CI 47 or newer](https://github.com/NuclearPowered/Reactor/actions/runs/723875068) for Among Us version 2021.3.31.3s).
3. Grab the [latest release](https://github.com/DorCoMaNdO/Reactor-Essentials/releases/latest) for your client version (support for older clients may be dropped, in that case browse [previous releases](https://github.com/DorCoMaNdO/Reactor-Essentials/releases)).
4. Place the downloaded release in `Among Us/BepInEx/plugins/` (same steps as installing Reactor).

## Development
To develop plugins with Essentials, Essentials needs to be installed, follow the steps above before proceeding.
Please configure notifications for future releases (`Watch` -> `Custom` -> `Releases`) to keep your projects up to date with bug fixes and new features.
This guide assumes Reactor.OxygenFilter.MSBuild is being used.
1. Open your project file (`.csproj`).
2. Add or locate an `ItemGroup` tag.
3. Add the following line: `<Deobfuscate Include="$(AmongUs)\BepInEx\plugins\Essentials-$(GameVersion).dll" />`
4. If using Visual Studio, building your project once with `dotnet build` may be required due to a Mono.Cecil issue present in Reactor.OxygenFilter.MSBuild.

## Building Essentials
Newer versions of Essentials use configurations based on Among Us target version(s) and override the `AmongUs` environment variable as a result.
Essentials depends on Reactor, follow installation steps 1 and 2 before proceeding.

### Building for a single Among Us version
1. [Set up the `AmongUs` environment variable](https://docs.reactor.gg/docs/basic/install_netsdk_example_template#setup-among-us-environment-variable)
2. Select and build the project configuration targeting your target version, `dotnet build -c CONFIGURATION` (where `CONFIGURATION` is the configuration name, ex: `S20210412`), may be required due to a Mono.Cecil issue present in Reactor.OxygenFilter.MSBuild.
3. The compiled binary will be copied to the `plugins` folder of your targeted Among Us version, as well as a `bin` folder in the solution's folder.

### Building for multiple Among Us versions
1. Add an environment variable named `AmongUsMods` targeting the parent folder that contains different Among Us versions in sub-folders, for example: `%AmongUsMods%\2021.4.12s` should lead to the `2021.4.12s` sub-folder.
2. Edit the `build all.bat` script (in the root folder of the solution) so that it would only contain the Among Us versions you're targeting.
3. Run `build all.bat` (alternatively, in Visual Studio you can use `Build` -> `Batch Build...` from the toolbar and select all the target versions and then `Build`, running `build all.bat` may still be required when Reactor is updated).
4. The compiled binary will be copied to the `plugins` folder of your targeted Among Us versions, as well as a `bin` folder in the solution's folder.