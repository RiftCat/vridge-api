using System;
using System.Runtime.CompilerServices;
using System.Timers;
using NetMQ;
using NetMQ.Sockets;
using VRE.Vridge.API.Client.Helpers;

namespace VRE.Vridge.API.Client.Proxy
{
    public class ClientProxyBase : IDisposable
    {        
        protected bool AutoRestartOnTimeout = false;                        

        private readonly string endpointAddress;
        private readonly bool shouldKeepAlive;

        private Timer keepAliveTimer;

        private RequestSocket socket;
        

        protected ClientProxyBase(string endpointAddress, bool keepAlive = false)
        {
            this.endpointAddress = endpointAddress;
            this.shouldKeepAlive = keepAlive;

            CreateSocket();                        
        }

        internal bool IsSocketOpen => socket != null;

        /// <summary>
        /// Sends a struct message and deserializes response bytes into response struct.
        /// No timeout handling.
        /// </summary>        
        public T OptimisticSendMessage<T>(object msg)
        {
            socket.SendFrame(Serialize(msg));
            var reply = socket.ReceiveFrameBytes();            

            return Deserialize<T>(reply);
        }

        /// <summary>
        /// Sends a struct message and deserializes response bytes into response struct.
        /// Throws System.TimeoutException if timeout occurs and closes socket.
        /// </summary>        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public T SendMessage<T>(object msg, int timeoutMs = 1000)
        {
            if (socket == null)
            {
                throw Timeout();
            }

            var stillAlive = socket.TrySendFrame(TimeSpan.FromMilliseconds(timeoutMs), Serialize(msg));

            if (!stillAlive)
            {
                throw Timeout();
            }

            stillAlive = socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(timeoutMs), out var reply);

            if (!stillAlive)
            {
                throw Timeout();                
            }

            return Deserialize<T>(reply);
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
                return Deserialize<T>(reply);
            }

            throw Timeout();            
        }

        /// <summary>
        /// Sends a zero byte to prevent connection from timing out.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendKeepAlivePing()
        {
            var isStillAlive = socket.TrySendFrame(TimeSpan.FromMilliseconds(30), new byte[]{0});

            if (!isStillAlive)
            {
                Timeout();
                return;
            }

            isStillAlive = socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(30), out var bytes);

            if (!isStillAlive)
            {
                Timeout();                
            }

        }

        private void CreateSocket()
        {
            socket = new RequestSocket();
            socket.Options.Linger = TimeSpan.FromSeconds(1);
            socket.Connect(endpointAddress);

            if (!shouldKeepAlive) return;

            /* Very basic keep alive mechanism
             * generally you should provide a steady stream of data yourself 
             * or reconnect after a period if inactivity */
            keepAliveTimer = new Timer(5000);
            keepAliveTimer.Elapsed += (s, e) => SendKeepAlivePing();
            keepAliveTimer.Start();
        }

        private TimeoutException Timeout()
        {
            CloseSocket();

            if (AutoRestartOnTimeout)
            {                
                CreateSocket();
            }

            return new TimeoutException();
        }


        protected void CloseSocket()
        {
            keepAliveTimer?.Stop();
            socket?.Dispose();
            socket = null;
        }

        protected virtual byte[] Serialize(object o)
        {
            return SerializationHelpers.StructureToByteArray(o);
        }

        protected virtual T Deserialize<T>(byte[] array)
        {
            return SerializationHelpers.ByteArrayToStructure<T>(array);
        }

        public void Dispose()
        {
            CloseSocket();            
        }
    }
}
