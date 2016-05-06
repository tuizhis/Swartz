using Swartz.Messaging;

namespace Swartz.Email
{
    public class DefaultEmailMessageChannelSelector : Component, IMessageChannelSelector
    {
        private readonly IWorkContextAccessor _workContextAccessor;

        public DefaultEmailMessageChannelSelector(IWorkContextAccessor workContextAccessor)
        {
            _workContextAccessor = workContextAccessor;
        }

        public MessageChannelSelectorResult GetChannel(MessagingType messagingType, object payload)
        {
            if (messagingType == MessagingType.Email)
            {
                var workContext = _workContextAccessor.GetContext();
                return new MessageChannelSelectorResult
                {
                    Priority = 50,
                    MessageChannel = () => workContext.Resolve<ISmtpChannel>()
                };
            }

            return null;
        }
    }
}