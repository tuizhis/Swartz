using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swartz.Messaging
{
    public interface IMessageChannel : IDependency
    {
        Task ProcessAsync(IDictionary<string, object> parameters);
    }
}