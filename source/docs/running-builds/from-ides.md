---
_disableAffix: true
jr.disableMetadata: false
jr.disableLeftMenu: false
jr.disableRightMenu: true
uid: running-builds-from-ides
title: From IDEs
---

# From IDEs

While NUKE is fully accessible from a clean IDE installation, there are extensions in place, which greatly enhance the way how targets can be invoked. However, depending on the individual capabilities of an IDE and their SDK, these extensions have a different look and feel.

## VisualStudio

In [VisualStudio](https://marketplace.visualstudio.com/items?itemName=nuke.visualstudio), targets can be executed from the **Task Runner Explorer** (_View | Other Windows_) by double clicking on of the tree nodes. Two buttons on the left allow to automatically **attach to the build process** and to **skip dependencies**.

![VisualStudio](~/images/visualstudio.png)

## ReSharper & Rider

In [Rider](https://plugins.jetbrains.com/plugin/10803-nuke-support) and [ReSharper](https://plugins.jetbrains.com/plugin/11804-nuke-support), targets are marked with a **gutter icon** and can be executed from the **Alt+Enter** menu. Selecting the root item will simply execute the target, while subitems allow to **attach the debugger** and to **skip dependencies**. A global action `GlobalNukeTargetExecution` can be used to invoke targets from anywhere in the solution. Rider will also create **dedicated run configurations** that allow further customizations, like working directory or environment variables.

![Rider](~/images/rider.png)

## VSCode

In [VSCode](https://marketplace.visualstudio.com/items?itemName=nuke.support), targets can be executed via **CodeLens items** that are shown above target declarations. The actions are limited to **Run and Debug**, but can be configured to include dependencies or not. Entering the **command-palette**, targets can be invoked from anywhere in the solution.

![VSCode](~/images/vscode.png)

