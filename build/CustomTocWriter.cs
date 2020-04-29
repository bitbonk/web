// Copyright Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/web/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.Bamboo;
using Nuke.Common.CI.Bitrise;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitLab;
using Nuke.Common.CI.Jenkins;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.CI.TravisCI;
using Nuke.Common.Git;
using Nuke.Common.Gitter;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.CoverallsNet;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotCover;
using Nuke.Common.Tools.DotMemoryUnit;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.DupFinder;
using Nuke.Common.Tools.EntityFramework;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitLink;
using Nuke.Common.Tools.GitReleaseManager;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.Helm;
using Nuke.Common.Tools.InnoSetup;
using Nuke.Common.Tools.InspectCode;
using Nuke.Common.Tools.Kubernetes;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Tools.NSwag;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.NUnit;
using Nuke.Common.Tools.Octopus;
using Nuke.Common.Tools.OpenCover;
using Nuke.Common.Tools.Paket;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Tools.SignTool;
using Nuke.Common.Tools.Slack;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Tools.SpecFlow;
using Nuke.Common.Tools.Squirrel;
using Nuke.Common.Tools.TestCloud;
using Nuke.Common.Tools.Twitter;
using Nuke.Common.Tools.Unity;
using Nuke.Common.Tools.VSTest;
using Nuke.Common.Tools.VSWhere;
using Nuke.Common.Tools.WebConfigTransformRunner;
using Nuke.Common.Tools.Xunit;
using static Nuke.Common.IO.SerializationTasks;
using static Nuke.Common.IO.PathConstruction;

static class CustomTocWriter
{
    [UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
    class Item
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public Item[] Items { get; set; }
    }

    static readonly Dictionary<string, Type[]> Items =
        new Dictionary<string, Type[]>()
        {
            {
                "Common",
                new[]
                {
                    typeof(ControlFlow),
                    typeof(EnvironmentInfo),
                    typeof(Logger),
                    typeof(NukeBuild),
                    typeof(ParameterAttribute)
                }
            },
            {
                "Execution",
                new[]
                {
                    typeof(ProcessTasks),
                    typeof(LocalExecutableAttribute),
                    typeof(PathExecutableAttribute),
                    typeof(PackageExecutableAttribute),
                    typeof(ToolPathResolver),
                    typeof(NuGetPackageResolver)
                }
            },
            {
                "IO",
                new[]
                {
                    typeof(PathConstruction),
                    typeof(AbsolutePath),
                    typeof(RelativePath),
                    typeof(WinRelativePath),
                    typeof(UnixRelativePath),
                    typeof(FileSystemTasks),
                    typeof(FtpTasks),
                    typeof(HttpTasks),
                    typeof(SerializationTasks),
                    typeof(CompressionTasks),
                    typeof(TextTasks),
                    typeof(XmlTasks)
                }
            },
            {
                "CI Interfaces",
                new[]
                {
                    typeof(AppVeyor),
                    typeof(AzurePipelines),
                    typeof(Bamboo),
                    typeof(Bitrise),
                    typeof(GitHubActions),
                    typeof(GitLab),
                    typeof(Jenkins),
                    typeof(TeamCity),
                    typeof(TravisCI)
                }
            },
            {
                "CI Configuration",
                new[]
                {
                    typeof(AppVeyorAttribute),
                    typeof(AzurePipelinesAttribute),
                    typeof(GitHubActionsAttribute),
                    typeof(TeamCityAttribute),
                    typeof(Partition)
                }
            },
            {
                "VCS",
                new[]
                {
                    typeof(GitTasks),
                    typeof(GitRepository),
                    typeof(GitRepositoryAttribute)
                }
            },
            {
                "Build & Code",
                new[]
                {
                    typeof(DotNetTasks),
                    typeof(MSBuildTasks),
                    typeof(ProjectModelTasks),
                    typeof(Solution),
                    typeof(SolutionAttribute),
                    typeof(VSWhereTasks)
                }
            },
            {
                "Release Management",
                new[]
                {
                    typeof(GitHubTasks),
                    typeof(GitVersionTasks),
                    typeof(GitVersionAttribute),
                    typeof(ChangelogTasks),
                    typeof(NpmTasks),
                    typeof(NuGetTasks),
                    typeof(PaketTasks),
                    typeof(DocFXTasks),
                    typeof(OctopusTasks),
                    typeof(SignToolTasks),
                    typeof(GitReleaseManagerTasks)
                }
            },
            {
                "Testing",
                new[]
                {
                    typeof(XunitTasks),
                    typeof(NUnitTasks),
                    typeof(SpecFlowTasks),
                    typeof(VSTestTasks),
                    typeof(MSBuildTasks),
                    typeof(TestCloudTasks)
                }
            },
            {
                "Coverage & Quality",
                new[]
                {
                    typeof(CoverletTasks),
                    typeof(DotCoverTasks),
                    typeof(DotMemoryUnitTasks),
                    typeof(DupFinderTasks),
                    typeof(InspectCodeTasks),
                    typeof(OpenCoverTasks),
                    typeof(SonarScannerTasks),
                    typeof(CoverallsNetTasks),
                    typeof(ReportGeneratorTasks)
                }
            },
            {
                "Virtualization",
                new[]
                {
                    typeof(DockerTasks),
                    typeof(KubernetesTasks),
                    typeof(HelmTasks),
                }
            },
            {
                "Social Media",
                new[]
                {
                    typeof(SlackTasks),
                    typeof(GitterTasks),
                    typeof(TwitterTasks),
                }
            },
            {
                "Others",
                new[]
                {
                    typeof(EntityFrameworkTasks),
                    typeof(GitLinkTasks),
                    typeof(InnoSetupTasks),
                    typeof(NSwagTasks),
                    typeof(SquirrelTasks),
                    typeof(UnityTasks),
                    typeof(WebConfigTransformRunnerTasks),
                }
            }
        };

    public static void WriteCustomTocs(
        AbsolutePath apiDirectory,
        AbsolutePath buildProjectDirectory,
        IEnumerable<string> assemblyFiles)
    {
        YamlSerializeToFile(
            Items.Select(x =>
                new Item
                {
                    Name = x.Key,
                    Items = x.Value.Select(y =>
                        new Item
                        {
                            Name = y.Name,
                            Uid = y.FullName
                        }).ToArray()
                }),
            apiDirectory / "toc.yml");
        // var assemblies = assemblyFiles.ForEachLazy(x => Info($"Loading {x}")).Select(AssemblyDefinition.ReadAssembly).ToList();
        // try
        // {
        //     var typeDefinitions = assemblies
        //         .SelectMany(x => x.MainModule.Types)
        //         .Distinct(x => x.FullName)
        //         .ToDictionary(x => x.FullName, x => x.Name);
        // }
        // finally
        // {
        //     assemblies.ForEach(x => x.Dispose());
        // }
    }
}
