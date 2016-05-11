using Swartz.Environment.Configuration;
using Swartz.Environment.ShellBuilders.Models;

namespace Swartz.Environment.ShellBuilders
{
    public interface ICompositionStrategy
    {
        ShellBlueprint Compose(ShellSettings settings);
    }
}