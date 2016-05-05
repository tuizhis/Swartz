using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swartz.Messaging
{
    public enum MessagingType
    {
        Email = 1,
        Sms = 2,
        Push = 3
    }

    public interface IMessageService : IDependency
    {
        Task SendAsync(MessagingType type, IDictionary<string, object> parameters);
    }
}