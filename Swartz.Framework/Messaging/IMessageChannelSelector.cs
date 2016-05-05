using System;

namespace Swartz.Messaging
{
    public interface IMessageChannelSelector : IDependency
    {
        MessageChannelSelectorResult GetChannel(MessagingType messagingType, object payload);
    }

    public class MessageChannelSelectorResult
    {
        public int Priority { get; set; }
        public Func<IMessageChannel> MessageChannel { get; set; }
    }
}