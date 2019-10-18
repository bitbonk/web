---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: getting-started-philosophy
title: Philosophy
---

# Philosophy

As .NET developers, we are already blessed with a variety of good options for build automation. This fact makes it very important for NUKE to thoroughly reflect on motivations and philosophies.

## Simplicity

Everything should be as simple as possible in all technical aspects. Builds are written in **pure C# console applications**, without any pre-processing or scripting bits. Inherently, this delivers code completion, navigation, debugging and other IDE features out-of-the-box. As a result, it is easily **approachable for everyone**, making build automation a team responsibility again. A nice side effect for NUKE itself, is that there is more time to work on actual build automation features.

## Extensibility

Of course, NUKE is meant to be easily configurable and extendable. Obviously, it supports common techniques like NuGet/Paket **package references and class inheritance**. On top of that, the NUKE **code generation** makes it easy to add support for additional CLI tools, that are not yet part of the library. The generated code provides a clean, consistent and powerful API for tools like MSBuild, Docker or Azure CLI. Other extension points include value-injection via custom attributes and log output adaptation for CI build servers.

## Community

We are recognizing the great potential of **collaborative work**. Our Slack workspace and GitHub are a great way to help each other, discuss new ideas and troubleshoot issues. All this helps to make NUKE as a tool more reliable, more powerful and maybe the number one tool in .NET build automation.

**Happy building!**
