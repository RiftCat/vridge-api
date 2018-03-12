using System;
using VRE.Vridge.API.Client.Proxy;

namespace VRE.Vridge.API.Client.Messages.Control.Responses
{
    public class EndpointCreated : ControlResponseHeader
    {
        public int Port { get; set; }

        /// <summary>
        /// Timeout after which the API server will disconnect you after not sending data for given amount of seconds.
        /// Use keep-alive (byte[1] {0}) packets if you want to stay silent but connected.
        /// See <see cref="ClientProxyBase.SendKeepAlivePing"/> for example impl.
        /// </summary>
        public int TimeoutSec { get; set; }
    }
}
