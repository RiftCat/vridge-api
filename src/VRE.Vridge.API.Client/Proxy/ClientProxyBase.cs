using System;
using System.Runtime.CompilerServices;
using System.Timers;
using NetMQ;
using NetMQ.Sockets;
using VRE.Vridge.API.Client.Helpers;

namespace VRE.Vridge.API.Client.Proxy
{
    public class ClientProxyBase
    {        
        private readonly RequestSocket socket;        
        private readonly Timer keepAliveTimer;
        private readonly byte[] keepAlivePing = { 0 };

        protected ClientProxyBase(string endpointAddress, bool keepAlive = false)
        {
            socket = new RequestSocket();         
            socket.Options.Linger = TimeSpan.FromSeconds(1);   
            socket.Connect(endpointAddress);
            
            if (!keepAlive) return;

            /* Very basic keep alive mechanism
             * generally you should provide a steady stream of data yourself 
             * or reconnect after a period if inactivity */
            keepAliveTimer = new Timer(5000);
            keepAliveTimer.Elapsed += (s, e) => SendKeepAlivePing();
            keepAliveTimer.Start();
        }

        /// <summary>
        /// Sends a struct message and deserializes response bytes into response struct.
        /// No timeout handling.
        /// </summary>        
        public T OptimisticSendMessage<T>(object msg)
        {
            socket.SendFrame(SerializationHelpers.StructureToByteArray(msg));
            var reply = socket.ReceiveFrameBytes();            

            return SerializationHelpers.ByteArrayToStructure<T>(reply);
        }

        /// <summary>
        /// Sends a struct message and deserializes response bytes into response struct.
        /// Throws System.TimeoutException if timeout occurs and closes socket.
        /// </summary>        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public T SendMessage<T>(object msg, int timeoutMs = 1000)
        {
            byte[] reply;

            socket.SendFrame(SerializationHelpers.StructureToByteArray(msg));
            var success = socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(timeoutMs), out reply);

            if (success)
            {
                return SerializationHelpers.ByteArrayToStructure<T>(reply);
            }

            socket.Close();
            throw new TimeoutException();            
        }

        /// <summary>
        /// Sends a struct message and deserializes response bytes into response struct.
        /// Throws System.TimeoutException if timeout occurs and closes socket.
        /// Skips inputserialization.
        /// </summary>     
        [MethodImpl(MethodImplOptions.Synchronized)]   
        public T SendRawFrame<T>(byte[] msgFrame, int timeoutMs = 1000)
        {
            byte[] reply;

            socket.SendFrame(msgFrame);
            var success = socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(timeoutMs), out reply);

            if (success)
            {
                return SerializationHelpers.ByteArrayToStructure<T>(reply);
            }

            socket.Close();
            throw new TimeoutException();
        }

        /// <summary>
        /// Sends a zero byte to prevent connection from timing out.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendKeepAlivePing()
        {
            socket.SendFrame(keepAlivePing);
            socket.ReceiveFrameBytes();
        }


        protected void CloseSocket()
        {
            keepAliveTimer?.Stop();
            socket.Dispose();
        }
    }
}
