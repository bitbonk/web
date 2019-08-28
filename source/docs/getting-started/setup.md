---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: getting-started-setup
title: Setup
---

# Setup

In order to setup a repository with NUKE, the global tool `Nuke.GlobalTool` must be installed:

```c#
$ dotnet tool install Nuke.GlobalTool --global

You can invoke the tool using the following command: nuke
Tool 'nuke.globaltool' (version '0.10.0') was successfully installed.
```

> [!Warning]
> When using ZSH as a shell, the dotnet tools path `$HOME/.dotnet/tools` must be added manually (see [dotnet/cli#9321](https://github.com/dotnet/cli/issues/9321)). This can be achieved by adding `export PATH=$HOME/.dotnet/tools:$PATH` to the `.zshrc` file.

Afterwards, the setup can be invoked with `nuke :setup`:

![Setup](~/images/setup.gif)

> [!Note]
> By default, NUKE will use the **.NET Core SDK** as bootstrapping method. Passing the `--boot` parameter, will reveal **.NET Framework/Mono** as another option. Note that even with .NET Core SDK bootstrapping, the build will be able to compile projects targeting .NET Framework. For further comparison, the template scripts [build.netcore.ps1](https://github.com/nuke-build/common/blob/develop/source/Nuke.GlobalTool/templates/build.netcore.ps1)/[build.netcore.sh](https://github.com/nuke-build/common/blob/develop/source/Nuke.GlobalTool/templates/build.netcore.sh) and [build.netfx.ps1](https://github.com/nuke-build/common/blob/develop/source/Nuke.GlobalTool/templates/build.netfx.ps1)/[build.netfx.sh](https://github.com/nuke-build/common/blob/develop/source/Nuke.GlobalTool/templates/build.netfx.sh) can be checked.

> [!Note]
> When using the .NET Core SDK bootstrapping, there is a possibility to **remove the bootstrapping files** `build.ps1` and `build.sh`. In that case, `dotnet run` can be called directly on the build project file. However, having the bootstrapping files in place ensures the availability of the dotnet CLI. If no local installation exists, it will be downloaded. Also this approach takes the `global.json` file into account, which allows to [define the SDK version](https://docs.microsoft.com/en-us/dotnet/core/tools/global-json).

When the setup has finished, the applied changes can be checked (e.g. `git status -s -uall`):

| File | Comment | 
| --- | --- |
| Added `.nuke` | Root directory marker file; references default solution file |
| Added `_build.csproj` | Build project file (console application) |
| Added `Build.cs` | Default `Build` class implementation |
| Added `build.ps1` | Bootstrapping for Windows/PowerShell |
| Added `build.sh` | Bootstrapping for Unix/Bash |
| Added `.editorconfig` | Roslyn code formatting settings |
| Added `.dotsettings` | ReSharper code formatting settings |
| Changed solution file | Include build project if chosen |

> [!Note]
> For maximum readability, NUKE prefers certain code formatting settings. For example, these include to omit accessibility modifiers and use expression-bodied properties. In case the build project should look like any other project, the two files `.editorconfig` and `.dotsettings` may simply be removed.

## Wizard

The setup can be continued with an extended wizard, which helps to scaffold the initial `Build` class according to best practices. For instance, it suggests to use [GitVersion](https://gitversion.readthedocs.io/) for generation of version numbers and provides a default implementation for the `Clean`, `Restore` and `Compile` targets.
