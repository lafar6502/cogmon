using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services
{
    /// <summary>
    /// For distributing application internal events
    /// </summary>
    public interface IMessageDispatcher
    {
        void Publish(object eventObj);
    }
}
