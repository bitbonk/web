---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: authoring-builds-system-paths
title: Path Handling
---

# System Paths

From time to time, incompliant paths can cause some non-obvious issues when executing builds. Directory separators need to match the underlying operating system, and relative paths can be messed up when using a different working directory. To solve this problem, NUKE implements several classes that make working with paths easier. A central idea of these classes is to override the division operator `/` to improve readability and hide details about directory separators at the same time.

NUKE advocates to use absolute paths whenever possible. For this purpose, the provided class `AbsolutePath` ensures that all its instances actually represent an absolute path, also known as _rooted path_. The base class `NukeBuild` defines several such instances, most importantly via the `RootDirectory` property:

```c#
class Build : NukeBuild
{
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath CommonProjectFile => SourceDirectory / "Common" / "Common.csproj";
}
```

For relative paths, the classes `RelativePath`, `WinRelativePath` and `UnixRelativePath` can be used. While `RelativePath` uses the directory separator used by the current operating system, `WinRelativePath` and `UnixRelativePath` will use `\` and `/` respectively.

All path related classes will automatically normalize and reduce the paths they're pointing to. For instance, `/bin/../foo\.\bar` will return `/foo/bar`. Moreover, they provide an implicit conversion to `string`, which allows flexible use with other APIs.
