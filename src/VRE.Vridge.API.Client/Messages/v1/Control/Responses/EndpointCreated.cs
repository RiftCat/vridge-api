using VRE.Vridge.API.Client.Proxy;

namespace VRE.Vridge.API.Client.Messages.v1.Control.Responses
{
    public class EndpointCreated : ControlResponseHeader
    {
        /// <summary>
        /// ZMQ REP server will await at this address. Use REQ socket to create connection.
        /// </summary>
        public string EndpointAddress { get; set; }

        /// <summary>
        /// Timeout after which the API server will disconnect you after not sending data for given amount of seconds.
        /// Use keep-alive (byte[1] {0}) packets if you want to stay silent but connected.
        /// See <see cref="ClientProxyBase.SendKeepAlivePing"/> for example impl.
        /// </summary>
        public int TimeoutSec { get; set; }
    }
}
