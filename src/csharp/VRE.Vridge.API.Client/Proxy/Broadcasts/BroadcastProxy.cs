using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NetMQ;
using NetMQ.Sockets;
using VRE.Vridge.API.Client.Messages.v3.Broadcast;

namespace VRE.Vridge.API.Client.Proxy.Broadcasts
{
    public class BroadcastProxy : IDisposable
    {
        private readonly SubscriberSocket socket;
        private readonly NetMQPoller poller;
        private bool isDisposed = false;

        public BroadcastProxy(string endpointAddr)
        {
            socket = new SubscriberSocket(endpointAddr);
            socket.Subscribe("haptic");
            socket.Options.Linger = TimeSpan.FromSeconds(1);
            socket.ReceiveReady += BroadcastReceived;

            poller = new NetMQPoller {socket};                                    
            poller.RunAsync();
        }

        public event EventHandler<HapticPulse> HapticPulseReceived;

        public void Disconnect()
        {
            if(poller != null && poller.IsRunning) poller.Stop();
            socket?.Close();

            HapticPulseReceived = null;
        }

        private void BroadcastReceived(object sender, NetMQSocketEventArgs ev)
        {
            var topic = ev.Socket.ReceiveFrameString();
            var msg = ev.Socket.ReceiveFrameBytes();

            if (topic == "haptic")
            {
                var hapticPulse = Helpers.SerializationHelpers.ProtoDeserialize<HapticPulse>(msg);
                HapticPulseReceived?.Invoke(this, hapticPulse);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    HapticPulseReceived = null;
                    Disconnect();
                }
            }
        }
    }
}
