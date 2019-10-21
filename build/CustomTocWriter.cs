// Copyright Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/web/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Logger;
using static Nuke.Common.IO.SerializationTasks;
using static Nuke.Common.IO.PathConstruction;

static class CustomTocWriter
{
    [UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
    class Item
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string Icon { get; set; }
        public string TopicUid { get; set; }
        public string Separator { get; set; }
        public Item[] Items { get; set; }
    }

    public static void WriteCustomTocs(
        AbsolutePath apiDirectory,
        AbsolutePath buildProjectDirectory,
        IEnumerable<string> assemblyFiles)
    {
        var assemblies = assemblyFiles.ForEachLazy(x => Info($"Loading {x}")).Select(AssemblyDefinition.ReadAssembly).ToList();
        try
        {
            var typeDefinitions = assemblies
                .SelectMany(x => x.MainModule.Types)
                .Distinct(x => x.FullName)
                .ToDictionary(x => x.FullName, x => x.Name);

            void ApplyName(Item item)
            {
                if (item.Uid != null)
                    item.Name = typeDefinitions.TryGetValue(item.Uid, out var name) ? name : "bla";

                if (item.Items == null)
                    return;
                
                foreach (var subItem in item.Items)
                    ApplyName(subItem);
            }

            var items = YamlDeserializeFromFile<Item[]>(buildProjectDirectory / "toc.template.yml");
            items.ForEach(ApplyName);
            YamlSerializeToFile(items, apiDirectory / "toc.yml");
        }
        finally
        {
            assemblies.ForEach(x => x.Dispose());
        }
    }
}
