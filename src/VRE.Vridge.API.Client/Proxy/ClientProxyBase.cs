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
            byte[] reply;

            socket.SendFrame(Serialize(msg));
            var success = socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(timeoutMs), out reply);

            if (success)
            {
                return Deserialize<T>(reply);
            }

            throw Timeout();            
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
            socket.SendFrame(new byte[]{0});
            socket.ReceiveFrameBytes();
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
            socket.Close();
            keepAliveTimer?.Stop();

            if (AutoRestartOnTimeout)
            {
                socket.Dispose();
                CreateSocket();
            }

            return new TimeoutException();
        }


        protected void CloseSocket()
        {
            keepAliveTimer?.Stop();
            socket.Dispose();
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
