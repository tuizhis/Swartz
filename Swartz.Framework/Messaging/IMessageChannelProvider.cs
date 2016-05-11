using System.Collections.Generic;
using System.Linq;

namespace Swartz.Messaging
{
    public interface IMessageChannelManager : IDependency
    {
        IMessageChannel GetMessageChannel(MessagingType type, IDictionary<string, object> parameters);
    }

    public class MessageChannelManager : IMessageChannelManager
    {
        private readonly IEnumerable<IMessageChannelSelector> _messageChannelSelectors;

        public MessageChannelManager(IEnumerable<IMessageChannelSelector> messageChannelSelectors)
        {
            _messageChannelSelectors = messageChannelSelectors;
        }

        public IMessageChannel GetMessageChannel(MessagingType type, IDictionary<string, object> parameters)
        {
            var messageChannelResult =
                _messageChannelSelectors.Select(x => x.GetChannel(type, parameters))
                    .Where(x => x != null)
                    .OrderByDescending(x => x.Priority)
                    .FirstOrDefault();

            return messageChannelResult?.MessageChannel();
        }
    }
}