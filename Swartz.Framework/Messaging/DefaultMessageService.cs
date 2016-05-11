using System.Collections.Generic;
using System.Threading.Tasks;
using Swartz.Logging;

namespace Swartz.Messaging
{
    public class DefaultMessageService : Component, IMessageService
    {
        private readonly IMessageChannelManager _messageChannelManager;

        public DefaultMessageService(IMessageChannelManager messageChannelManager)
        {
            _messageChannelManager = messageChannelManager;
        }

        public async Task SendAsync(MessagingType type, IDictionary<string, object> parameters)
        {
            var messageChannel = _messageChannelManager.GetMessageChannel(type, parameters);

            if (messageChannel == null)
            {
                Logger.Warning($"No channels where found to process a message of type {type}");
                return;
            }

            await messageChannel.ProcessAsync(parameters);
        }
    }
}