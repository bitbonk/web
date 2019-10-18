---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: authoring-builds-cli-tools
title: CLI Tools
---

# CLI Tools

Interacting with third-party command-line interface tools (CLIs) is an essential task in build automation. This includes resolution of the tool path, construction of arguments to be passed, evaluation of the exit code and capturing of standard and error output. NUKE can hide those trivialities in dedicated auto-generated CLI task classes. For instance, calling MSBuild can be done as follows:

```c#
// using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

MSBuild($"{SolutionFile} /target:Rebuild /p:Configuration={Configuration} /nr:false");
```

The returned object is a collection of standard and error output.

> [!Note]
> Most CLI tasks require to add a package reference to the build project file. For instance, when using `NUnitTasks` there should be an entry `<PackageReference Include="NUnit.ConsoleRunner" Version="3.9.0" />` or `<PackageDownload Include="NUnit.ConsoleRunner" Version="[3.9.0]" />` in the project file. While it would be possible to magically download required packages, this approach ensures reproducible builds at any time. If a package reference/download is missing, the resulting error message will contain its actual package id.

## Fluent APIs

While the example from above is quite easy to understand, it also illustrates certain weaknesses. What if `SolutionFile` contains a space? How can multiple targets be passed? Should the configuration really be injected as property or as dedicated argument? What does the `/nr` switch stand for? These issues can be solved by using individual fluent APIs:

```c#
// using Nuke.Common.Tools.MSBuild;
// using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

MSBuild(o => o
    .SetTargetPath(SolutionFile)
    .SetTargets("Clean", "Build")
    .SetConfiguration(Configuration)
    .EnableNodeReuse());
```

These fluent APIs are used to manipulate the process invocation, including tool path, arguments, working directory, timeout and environment variables.

> [!Note]
> All fluent APIs implement a variation of the [builder pattern](https://en.wikipedia.org/wiki/Builder_pattern), in which every fluent call will create an immutable copy of the current `ToolSettings` instance with the intended changes applied. This enables to compose similar process invocations very easily by reusing an intermediate state of the settings object.

Using any IDE, an individual fluent API can easily be discovered via code completion. Most importantly, this contains the original tool documentation:

![CLI Tools](~/images/cli-tools.gif)

### Conditional Modifications

Sometimes it is desirable to set certain options only, if some condition is met. This can be done fluently too, using the `When` extension:

```c#
DotNetTest(o => o
    .SetProjectFile(ProjectFile)
    .SetFramework("netcoreapp2.0")
    .SetConfiguration(Configuration)
    .EnableNoBuild()
    .When(PublishTestResults, oo => oo
        .SetLogger("trx")
        .SetResultsDirectory(TestResultsDirectory)));
```

### Combinatorial Modifications

A typical situation when using MSBuild for compilation, is to compile for different configurations, target frameworks or runtimes. This can easily be done using the `CombineWith` method:

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

Depending on the number of target frameworks defined in the project files, there will be a minimum of 6 invocations of `dotnet publish`.

There is also an overload to create more individual continuations:

```c#
DotNetPublish(o => o
    .SetConfiguration(Configuration)
    .EnableNoRestore()
    .CombineWith(
        oo => oo
            .SetProject(FirstProject),
        oo => oo
            .SetProject(SecondProject)
            .SetFramework("netstandard2.0"),
        oo => oo
            .SetProject(SecondProject)
            .SetFramework("net461")));
```

### Multiple Invocations

Based on [combinatorial modifications](#combinatorial-modifications) it is possible to set a `degreeOfParallelism` (default `1`) and a flag to `continueOnFailure` (default `false`):

```c#
DotNetNuGetPush(s => s
        .SetSource(Source)
        .SetSymbolSource(SymbolSource)
        .SetApiKey(ApiKey)
        .CombineWith(
            OutputDirectory.GlobFiles("*.nupkg").NotEmpty(), (cs, v) => cs
                .SetTargetPath(v)),
    degreeOfParallelism: 5,
    continueOnFailure: true);
```

This example will always have 5 packages being pushed simultaneously. Possible exceptions, for instance when a package already exists, are accumulated to an `AggregateException` and thrown when all invocations have been processed. The console output is buffered until all invocations are completed.

### Custom Logging

The output of process execution is logged with level `Normal` for output from the standard stream, and `Error` from the error stream. In some cases, it's can be either helpful or even necessary to override this behavior. For instance, some tools (e.g., `git`) are using the error stream for standard messages. Other tools prefix their messages to indicate warnings or debug information. In these scenarios, the `ProcessTasks.DefaultLogger` can be replaced:

```c#
GitLogger = (output, type) => Logger.Normal(output);
```

Note, that replacing a logger will affect all invocations related to this tool.

### Verbosity Mapping

Using the `VerbosityMappingAttribute`, it is possible to automatically map the verbosity passed via `--verbosity` to individual tools. The attribute must be applied on the build class level:

```c#
[VerbosityMapping(typeof(MSBuildVerbosity),
    Quiet = nameof(MSBuildVerbosity.Quiet),
    Minimal = nameof(MSBuildVerbosity.Minimal),
    Normal = nameof(MSBuildVerbosity.Normal),
    Verbose = nameof(MSBuildVerbosity.Detailed))]
class Build : NukeBuild
```

For some tools there already exist predefined attributes, like `DefaultMSBuildVerbosityMapping` or `DefaultDotNetVerbosityMapping`.

### Custom Arguments

Occasionally, it may happen that an argument is not available from the fluent API. In this case, the `SetArgumentConfigurator` method can be used to add them manually:

```c#
MSBuild(o => o
    .SetTargetPath(SolutionFile)
    .SetArgumentConfigurator(a => a.Add("/r")));
```

<!--
    SetToolPath
    SetWorkingDirectory
    SetExecutionTimeout
    SetEnvironmentVariables
    LogOutput
    When
    SetArgumentConfigurator
-->

## Lightweight Integration

Many of the most popular tools for .NET development are already implemented either in `Nuke.Common` or dedicated `Nuke.[Tool]` packages. In case that a certain tool is not yet supported with a proper CLI task class, NUKE allows to use the `ToolResolver` or one of the following **injection attributes**:

```c#
[PathExecutable]
readonly Tool Git;

[LocalExecutable("./tools/corflags.exe")]
readonly Tool CorFlags;

[PackageExecutable(
    packageId: "xunit.runner.console",
    packageExecutable: "xunit.console.exe")]
readonly Tool Xunit;
```

The injected `Tool` delegate allows to pass arguments, working directory, environment variables and many more process-specific options:

```c#
Git($"checkout -b {Branch}", workingDirectory: checkoutDirectory);
```

Whenever it is desirable to have the same fluent syntax available for a custom tool, there is a dedicated article about [code generation](../writing-addons/code-generation.md) with further information.
