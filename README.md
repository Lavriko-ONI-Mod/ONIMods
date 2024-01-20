# Peterhaneve ONIMods fork for Lavriko

This repository is only needed to build PLib with added functionality

### Build scripts modified from the original to only build the PLib

# Building

I used .NET 8 SDK with .NET Framework 4.7.1 SDK installed.

You can check if `C://Windows/Microsoft.NET/Framework64/v4.0.30319` exists - then you're good to go.

This project only uses PLib projects, so other mods are NOT building.

Project opens and PLib builds in Rider 2023.1.2

There is a `custom_build.sh` script, that contains all logic about building PLib.

No damn MSBuild tasks and fancy build variables.

All you have to do is set a `GameFolderDefault` in `Directory.Build.props.default` to a folder with your game.

Do the same for `custom_build.sh`

I do know that Peter asked to create `.user` file, but I found it easier for myself to do it this way.