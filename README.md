# Essentials
Essentials is a modding library for Among Us with APIs to speed up and ease development, and to improve mod compatibility.

## Installation
**NOTE:** At the moment, official builds are only compiled for the Steam client.
1. Install [BepInEx](https://docs.reactor.gg/docs/basic/install_bepinex).
2. Install [Reactor](https://docs.reactor.gg/docs/basic/install_reactor) (CI 38 or newer for Essentials 0.1.1+, CI 45 for Among Us version 2021.3.5s).
3. Grab the [latest release](https://github.com/DorCoMaNdO/Reactor-Essentials/releases/latest) for your client version (support for older clients may be dropped, in that case browse [previous releases](https://github.com/DorCoMaNdO/Reactor-Essentials/releases)).
4. Place the downloaded release in `Among Us/BepInEx/plugins/` (same steps as installing Reactor).

## Development
To develop plugins with Essentials, Essentials needs to be installed, follow the steps above before proceeding.
This guide assumes Reactor.OxygenFilter.MSBuild is being used.
1. Open your project file (`.csproj`).
2. Add or locate an `ItemGroup` tag.
3. Add the following line: `<Deobfuscate Include="$(AmongUs)\BepInEx\plugins\Essentials-$(GameVersion).dll" />`
4. If using Visual Studio, building your project once with `dotnet build` may be required due to a Mono.Cecil issue present in Reactor.OxygenFilter.MSBuild.

## Building Essentials
Newer versions of Essentials use configurations based on Among Us target version(s) and override the `AmongUs` environment variable as a result.
Essentials depends on Reactor, follow installation steps 1 and 2 before proceeding.
1. Add an environment variable for your targeted Among Us version(s), the environment variable needs to be prefixed with `AmongUs_` and then be followed by the client version, with dashes substituting dots, for example: `AmongUs_2020-12-9s` for version 2020.12.9s.
2. Select the configuration for the targeted Among Us version (in Visual Studio, if building for more than one version, you can use Build -> Batch Build... from the toolbar and select all the target versions and then `Build`).
3. The compiled binary will be copied to the `plugins` folder of your targeted Among Us version(s), as well as a `bin` folder in the solution's folder.