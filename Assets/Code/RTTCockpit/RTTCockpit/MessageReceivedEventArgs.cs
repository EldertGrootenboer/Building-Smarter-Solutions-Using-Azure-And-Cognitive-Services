using Microsoft.Azure.ServiceBus;
using System;

namespace RTTCockpit
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }

        public MessageReceivedEventArgs(Message message)
        {
            Message = message;
        }
    }
}
