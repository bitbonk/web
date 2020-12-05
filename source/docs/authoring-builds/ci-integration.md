---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: authoring-builds-ci-integration
title: CI Integration
---

# CI Integration

Eventually, builds are supposed to be executed in a continuous integration environment. That means that every push will trigger a build to verify committed changes. NUKE aims to seamlessly integrate with those CI systems, for instance by allowing to have typed access to environment variables, reporting warnings and errors in the required format, or publishing artifacts via method calls.

## Configuration Generation

_Supported for TeamCity, Azure Pipelines, AppVeyor, GitHub Actions._

NUKE goes one step further and allows to conveniently generate the related configuration files, for instance YML files, by using _configuration generation_ attributes. Typically, these attributes are applied to the build class:

```c#
[TeamCity(
    TeamCityAgentPlatform.Windows,
    DefaultBranch = DevelopBranch,
    VcsTriggeredTargets = new[] { nameof(Pack), nameof(Test) },
    NightlyTriggeredTargets = new[] { nameof(Test) })]
[AzurePipelines(
    AzurePipelinesImage.UbuntuLatest,
    AzurePipelinesImage.WindowsLatest,
    AzurePipelinesImage.MacOsLatest,
    InvokedTargets = new[] { nameof(Test), nameof(Pack) })]
class Build : NukeBuild
{
}
```

Using the `nameof` operator for targets and parameters ensures refactoring-safety and that the configuration files are always up-to-date with the actual implementation. After changing the build configuration (e.g., renaming targets), the build must be triggered once, for instance by calling `nuke --help`. If any of the configuration files have changed, a warning is reported:

```c#
$ nuke --help

NUKE Execution Engine version 1.0.0 (OSX,.NETStandard,Version=v2.0)

Configuration files for TeamCity have changed.
Configuration files for AzurePipelines have changed.
```

> [!Warning]
> In TeamCity, _Import settings from .teamcity/settings.kts_ must be chosen during project creation. Afterwards, _Versioned Settings_ must be enabled as follows:
> ![TeamCity Versioned Settings](~/images/teamcity-versioned-settings.png)

For TeamCity and Azure Pipelines, the generated configuration takes advantage of the target dependency model. That means that for every target, a separate build configuration (TeamCity) or job (Azure Pipelines) is created. This provides a better overview for individual target behavior:

![Azure Pipelines Stages](~/images/azure-stages.png)

### Artifacts

_Supported for TeamCity, Azure Pipelines, AppVeyor, GitHub Actions._

Usually, builds are supposed to publish some kind of artifacts, like NuGet packages or test results. Using the `Produces` call, artifact paths can be defined per target:

```c#
Target Pack => _ => _
    .Produces(OutputDirectory / "*.nupkg")
    .Executes(() =>
    {
        DotNetPack(s => s
            .SetProject(Solution)
            .SetOutputDirectory(OutputDirectory));
    });
```

Defining artifact paths with [absolute paths](system-paths.md) is the recommended approach. In the resulting configuration files, they are automatically converted to relative paths.

For multi-staged builds with TeamCity, a target can consume the artifacts from another target by using `Produces` and `Consumes` in combination:

```c#
Target Restore => _ => _
    .Produces(SourceDirectory / "*/obj/**/*")
    .Executes(() =>
    {
    });

Target Compile => _ => _
    .Consumes(Restore)
    .Executes(() =>
    {
    });
```

### Partitioning

_Supported for TeamCity, Azure Pipelines._

Many targets are well-suited to be split into multiple partitions. For instance, think of a target that executes tests for several test assemblies. NUKE introduces an easy way to run those tests in parallel on different agents:

```c#
[Partition(2)] readonly Partition TestPartition;

Target Test => _ => _
    .DependsOn(Compile)
    .Partition(() => TestPartition)
    .Executes(() =>
    {
        var projects = Solution.GetProjects("*.Tests");
        var relevantProjects = TestPartition.GetCurrent(projects);        
        DotNetTest(s => s
            .SetConfiguration(Configuration)
            .CombineWith(
                relevantProjects, (cs, v) => cs
                    .SetProjectFile(v)));
    });
```

Adding the `Partition` attribute on the `TestPartition` field will automatically split the execution in to the specified amount of partitions. A partition can be associated with a target via `Partition(() => Partition)`. Calling `TestPartition.GetCurrent(enumerable)` inside the target implementation returns only the relevant items for the current partition. In terms of configuration files, this is implemented by adding the `--test-partition n` parameter. For local executions, the `TestPartition` has a size of `1`.

In TeamCity, the resulting build chain would look like this:

![TeamCity Build Chain](~/images/teamcity-build-chain.png)

### Parameters

_Supported for TeamCity._

All properties and fields having the `ParameterAttribute` applied, will automatically be exposed to the _Run Build Type_ dialog.

![TeamCity Dialog](~/images/teamcity-dialog.png)

Required parameters are marked with red asterisks. Default values are read from the initializers. Enumeration parameters can be picked from a drop-down.

### Serialization

_Work in progress. This will allow state to be shared when targets are executed on different agents._
