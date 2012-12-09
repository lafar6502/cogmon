using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.MessageBus.Impl;

namespace CogMon.Services.Util
{
    public class NGinnMessageDispatcher : IMessageDispatcher
    {
        public MessageDispatcher NGinnDispatcher { get; set; }
        
        public void Publish(object eventObj)
        {
            NGinnDispatcher.DispatchMessage(eventObj, null);
        }
    }
}
