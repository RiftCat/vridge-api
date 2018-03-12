using System;
using System.Runtime.CompilerServices;
using System.Timers;
using NetMQ;
using NetMQ.Sockets;
using VRE.Vridge.API.Client.Helpers;

namespace VRE.Vridge.API.Client.Proxy
{
    /// <summary>
    /// Updated to use ProtoBufs instead of default C# marshalling.
    /// </summary>
    public class ClientProxyBasePB : ClientProxyBase
    {
        public ClientProxyBasePB(string endpointAddress, bool keepAlive = false) : base(endpointAddress, keepAlive) { }

        protected sealed override byte[] Serialize(object o)
        {
            return SerializationHelpers.ProtoSerialize(o);
        }

        protected sealed override T Deserialize<T>(byte[] array)
        {
            return SerializationHelpers.ProtoDeserialize<T>(array);
        }
    }
}
