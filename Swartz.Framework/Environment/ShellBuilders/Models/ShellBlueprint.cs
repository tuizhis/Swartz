using System;
using System.Collections.Generic;

namespace Swartz.Environment.ShellBuilders.Models
{
    public class ShellBlueprint
    {
        public IEnumerable<ShellBlueprintItem> Dependencies { get; set; }

        public IEnumerable<ControllerBlueprint> Controllers { get; set; }

        public IEnumerable<ControllerBlueprint> HttpControllers { get; set; }

        public IEnumerable<ShellBlueprintItem> Modules { get; set; }
    }

    public class ShellBlueprintItem
    {
        public Type Type { get; set; }
    }

    public class ControllerBlueprint : ShellBlueprintItem
    {
        public string AreaName { get; set; }

        public string ControllerName { get; set; }
    }
}