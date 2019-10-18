---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: getting-started-faq
title: FAQ
---

# Frequently Asked Questions

- [How does NUKE compare to other build systems?](#how-does-nuke-compare-to-other-build-systems)
- [Do I really need the build.ps1 and build.sh files?](#do-i-really-need-the-buildps1-and-buildsh-files)
- [That `=> _ => _` looks weird. Can it be changed?](#that-----looks-weird-can-it-be-changed)
- [How can I debug my build?](#how-can-i-debug-my-build)
- [How can I execute tools from PATH?](#how-can-i-execute-tools-from-path)
- [How can I execute tools from NuGet packages?](#how-can-i-execute-tools-from-nuget-packages)
- [Why are my targets executed out of order?](#why-are-my-targets-executed-out-of-order)
- [How can I test/publish for multiple configurations/frameworks/runtimes?](#how-can-i-testpublish-for-multiple-configurationsframeworksruntimes)

### How does NUKE compare to other build systems?

For some candidates, obviously the host language differs. While NUKE is not limited to C# nor the .NET ecosystem, it is a reasonable consideration to pick a build system - and host language - that can be understood by everyone in a team. Talking about specific features:

- Pure C# without custom directives, pre-processing or scripting involved
- Native IDE support including code-completion, debugging, navigation, refactorings
- Targets as symbols instead of magic strings (by design)
- Declarative parameter resolution from command-line or environment variables
- Global tool with shell-completion for targets, parameter and enumerations
- Execution, ordering and trigger dependencies (including reverse definition)
- Support extensions for all major IDEs, for easier execution and faster authoring
- Rich and flexible APIs for third-party CLI tools (auto-generated)
- Execution plan visualization via HTML
- First-class techniques for build sharing

### Do I really need the build.ps1 and build.sh files?

No. It's perfectly fine to remove them and call `dotnet run` on the build project. However, having the bootstrapping files ensures the availability of the dotnet SDK. If there is no local installation, it will be downloaded to the temporary directory. This greatly simplifies execution on CI build servers.

### That `=> _ => _` looks weird. Can it be changed?

Firstly, we acknowledge that it looks funky, but at the same time we don't consider it being significant for working with NUKE. There are snippets available that help to write that part of target declarations more easily. Regarding possible changes, it is open for extension by overriding `NukeBuild.CreateExecutableTargetFactory()` in a [separate feature branch](https://github.com/nuke-build/common/tree/feature/target-factory).

### How can I debug my build?

Build implementations reside in simple console applications. So setting the build project as the startup project will immediately allow debugging. Also, using `Debugger.Launch` works, if you prefer to start the build from a shell. The best experience can be achieved by using one of the IDE extensions.

### How can I execute tools from PATH?

Most conveniently, global executables can be accessed by using the `PathExecutable` attribute:

```c#
[PathExecutable] readonly Tool Echo;

Target Foo => _ => _
    .Executes(() =>
    {
        Echo("arguments");
    });
```

### How can I execute tools from NuGet packages?

In order to execute tools from NuGet packages, the required package must be added as package reference. Yes, even if there is already a reference to `Nuke.Addon.[Tool]`. This approach ensures better versioning control. In absence of a dedicated fluent API, the `PackageExecutable` attribute can be used:

```c#
[PackageExecutable(
    packageId: "xunit.runner.console",
    packageExecutable: "xunit.console.exe")]
readonly Tool Xunit;

Target Foo => _ => _
    .Executes(() =>
    {
        Xunit("arguments");
    });
```

For more control, `ToolPathResolver`, `NuGetPackageResolver`, or `PaketPackageResolver` can be used.

### Why are my targets executed out of order?

It is very likely that wrong assumptions have been made. Calling `nuke --plan` will show a visual representation of all execution plans. Dependencies are solely defined at the individual targets. I.e., not by the order being passed from the command-line nor in which order they're mentioned in a `DependsOn` call and alike.

### How can I test/publish for multiple configurations/frameworks/runtimes?

By using the `CombineWith` extension method that is available to all `ToolSettings` derived types:

```c#
var publishCombinations =
    from project in new[] { FirstProject, SecondProject }
    from framework in project.GetMSBuildProject().GetTargetFrameworks()
    from runtime in new[] { "win10-x86", "osx-x64", "linux-x64" }
    select new { project, framework, runtime };

DotNetPublish(o => o
    .EnableNoRestore()
    .SetConfiguration(Configuration)
    .CombineWith(publishCombinations, (oo, v) => oo
        .SetProject(v.project)
        .SetFramework(v.framework)
        .SetRuntime(v.runtime)));
```

<!--
- Why is _external tool_ not working with NUKE?
- Is NUKE limited to .NET tools?
-->
