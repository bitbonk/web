---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: writing-addons-code-generation
title: Code Generation
---

# Code Generation

All [CLI tools](../authoring-builds/cli-tools.md) are implemented following a code generation approach

## Names

Names of the resulting tasks and properties should be as close as possible to the original CLI tool invocations. The top-level tool `name` will generate a `[Name]Tasks` class ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/EntityFramework.json#L6)). If the CLI tool has no sub-commands, the `tasks` collection will only have one entry, resulting in a single method declaration `[Name]Tasks.[Name]` ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/MSpec.json#L11-L105)). Otherwise, multiple objects with matching `postfix` and `definiteArgument` must be provided, which will generate `[Name]Tasks.[Name][Postfix]` methods ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/EntityFramework.json#L53-L64)).

## Official URL

The `officialUrl` field is mandatory ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/NuGet.json#L10)). Fluent APIs are intended to provide confidence. When in doubt, it must be easy to access the original tool website to get more detailed information.

## Documentation

All available documentation must be provided via the `help` field. This applies to the tool ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/Npm.json#L9)), the single tasks ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/Npm.json#L13)), and for all properties ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/Npm.json#L22)). HTML tags must be used for texts; links with `<a>`, lists with `<ul>` and `<li>`, emphasized text with `<em>`, code with `<c>`, and paragraphs with `<p>` ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotNet.json#L125)).

## Path Resolution

- `pathExecutable` resolves executables just like `which` and `where` do, i.e., by checking the `PATH` environment variable ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/Git.json#L6)).
- `packageId` and `packageExecutable` resolve the path by first checking the build project file for the _package ID_ and then using the supplied version to determine the path of the extracted NuGet package, and then searching the `./tools` directory for the given _package executable_ ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotCover.json#L9-L10)). Both fields can receive pipe-separated values to produce a fallback behavior ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/GitVersion.json#L10-L11)).
- `customExecutable` is a flag which will cause the generator to call `GetToolPath`, which needs to be implemented manually in a separate file ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/MSBuild.json#L9)).

## Property Types

- `object`, `string` and numeric types will generate `Set[Name]`, and `Reset[Name]` methods ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotNet.json#L37-L42))
- `bool` will additionally generate `Enable[Name]`, `Disable[Name]`, and `Toggle[Name]` methods ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotNet.json#L79-L84))
- `List<T>` will additionally generate `Add[Name]`, `Remove[Name]`, and `Clear[Name]` methods ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotNet.json#L562-L567))
- `Dictionary<TKey, TValue>` will additionally generate `Set[Name]`, `Add[Name]`, `Remove[Name]`, and `Clear[Name]` methods ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotNet.json#L695-L805))
- `LookupTable<TKey, TValue>` will additionally generate `Set[Name]`, `Add[Name]`, `Remove[Name]`, and `Clear[Name]` methods ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/Xunit.json#L16-L22))

The names for lists, dictionaries and lookup tables must be pluralized.

## Parameter Format

Readable parameters are to be preferred over shorthands, i.e., `--configuration` over `-c`.

## Enumerations

If a parameter can only receive a finite set, those values must be encapsulated in an `enumeration` ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/ReportGenerator.json#L85-L118)), which can be used as property type ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/Xunit.json#L145-L151)).

## Shared Properties

If a tool provides several sub-commands that share a reasonable number of properties, those can be moved to the `commonTaskProperties` that are automatically applied to all tasks ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/Paket.json#L290-L315)), or `commonPropertySets` ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotNet.json#L876-L883)) that can be applied by reference ([example](https://github.com/nuke-build/common/blob/develop/build/specifications/DotNet.json#L24-L27)).
