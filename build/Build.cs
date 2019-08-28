// Copyright Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/web/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using FluentFTP;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using Nuke.DocFX;
using static CustomTocWriter;
using static Disclaimer;
using static CustomDocFx;
using static Nuke.Common.IO.SerializationTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.FtpTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.DocFX.DocFXTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.BuildSite);

    [Parameter] readonly string FtpUsername;
    [Parameter] readonly string FtpPassword;
    [Parameter] readonly string FtpServer;

    [Parameter] readonly bool PublishDocs;
    [Parameter] readonly bool PublishImages;
    [Parameter] readonly bool PublishApi;

    new AbsolutePath OutputDirectory => RootDirectory / "output";
    new AbsolutePath SourceDirectory => RootDirectory / "source";

    AbsolutePath GenerationDirectory => TemporaryDirectory / "packages";
    AbsolutePath ApiDirectory => SourceDirectory / "api";

    string DocFxFile => RootDirectory / "docfx.json";
    AbsolutePath SiteDirectory => OutputDirectory / "site";

    [Solution] readonly Solution Solution;

    IEnumerable<ApiProject> Projects => YamlDeserializeFromFile<List<ApiProject>>(RootDirectory / "projects.yml")
                                        ?? new List<ApiProject>();

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("*/obj", "*/bin").ForEach(DeleteDirectory);
            DeleteDirectory(Solution.Directory / "obj");
            EnsureCleanDirectory(ApiDirectory);
            EnsureCleanDirectory(GenerationDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target DownloadPackages => _ => _
        .DependsOn(Clean)
        .OnlyWhenStatic(() => PublishApi)
        .WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            var packages = Projects.Select(x => x.PackageId).Concat("System.ValueTuple");
            packages.ForEach(x => NuGetTasks.NuGet($"install {x} -OutputDirectory {GenerationDirectory} -ExcludeVersion -DependencyVersion Ignore -Verbosity detailed"));
        });

    Target CustomDocFx => _ => _
        .DependsOn(DownloadPackages)
        .Executes(() =>
        {
            WriteCustomDocFx(DocFxFile, BuildProjectDirectory / "docfx.template.json", GenerationDirectory, ApiDirectory);
        });

    Target Disclaimer => _ => _
        .DependsOn(DownloadPackages)
        .OnlyWhenStatic(() => PublishApi)
        .WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            var disclaimerDirectory = SourceDirectory / "disclaimers";
            Directory.CreateDirectory(disclaimerDirectory);
            Projects.Where(x => x.IsExternalRepository)
                .ForEachLazy(x => Info($"Writing disclaimer for {x.PackageId}..."))
                .ForEach(x => WriteDisclaimer(x,
                    disclaimerDirectory / $"{x.PackageId}.disclaimer.md",
                    GlobFiles(GenerationDirectory / x.PackageId, "lib/net4*/*.dll")));
        });

    Target Metadata => _ => _
        .DependsOn(DownloadPackages, CustomDocFx)
        .OnlyWhenStatic(() => PublishApi)
        .WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            DocFXMetadata(s => s
                .SetProjects(DocFxFile)
                .SetLogLevel(DocFXLogLevel.Verbose));
        });

    Target CustomToc => _ => _
        .DependsOn(DownloadPackages)
        .After(Metadata)
        .Executes(() =>
        {
            GlobFiles(ApiDirectory, "**/toc.yml").ForEach(File.Delete);
            WriteCustomTocs(ApiDirectory, BuildProjectDirectory, GlobFiles(GenerationDirectory, "**/lib/net4*/*.dll"));
        });

    Target BuildSite => _ => _
        .DependsOn(Metadata, CustomToc, Disclaimer)
        .Executes(() =>
        {
            DocFXBuild(s => s
                .SetConfigFile(DocFxFile)
                .SetLogLevel(DocFXLogLevel.Verbose)
                .SetServe(InvokedTargets.Contains(BuildSite)));
        });

    Target Publish => _ => _
        .DependsOn(BuildSite)
        .Requires(() => FtpUsername, () => FtpPassword, () => FtpServer)
        .Executes(() =>
        {
            FtpCredentials = new NetworkCredential(FtpUsername, FtpPassword);

            if (PublishDocs)
                FtpUploadDirectoryRecursively(SiteDirectory / "docs", FtpServer + "/docs");
            if (PublishImages)
                FtpUploadDirectoryRecursively(SiteDirectory / "images", FtpServer + "/images");
            if (PublishApi)
                FtpUploadDirectoryRecursively(SiteDirectory / "api", FtpServer + "/api");


            return;
            var client = new FtpClient(FtpServer, new NetworkCredential(FtpUsername, FtpPassword));
            client.Connect();

            Directory.GetDirectories(SiteDirectory, "*", SearchOption.AllDirectories)
                .ForEach(directory =>
                {
                    var files = GlobFiles(directory, "*").ToArray();
                    var relativePath = GetRelativePath(SiteDirectory, directory);
                    var uploadedFiles = client.UploadFiles(files, relativePath, verifyOptions: FtpVerify.Retry);
                    ControlFlow.Assert(uploadedFiles == files.Length, "uploadedFiles == files.Length");
                });
        });
}
