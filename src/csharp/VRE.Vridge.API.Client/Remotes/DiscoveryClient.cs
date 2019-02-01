using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetMQ;
using ProtoBuf;
using VRE.Vridge.API.Client.Messages.v3.Discovery;
using VRE.Vridge.API.Client.Remotes.Beacons;

namespace VRE.Vridge.API.Client.Remotes
{
    internal class DiscoveryClient
    {
        private readonly VridgeServerBeaconList beaconList;
        private readonly NetMQBeacon beaconClient;
        private readonly NetMQPoller beaconPoller;

        public DiscoveryClient()
        {
            beaconList = new VridgeServerBeaconList();
            beaconClient = new NetMQBeacon();
            beaconClient.ConfigureAllInterfaces(38219);
            beaconClient.Subscribe("");
            beaconClient.ReceiveReady += OnBeaconReceived;
            beaconPoller = new NetMQPoller() { beaconClient };
            beaconPoller.RunAsync();
        }

        public List<VridgeServerBeacon> ActiveVridgeServers => beaconList.FreshServers;

        public void Dispose()
        {
            if(beaconPoller != null && beaconPoller.IsRunning) beaconPoller.Stop();
            beaconClient?.Dispose();
        }

        private void OnBeaconReceived(object sender, NetMQBeaconEventArgs e)
        {
            var beaconMessage = e.Beacon.Receive();
            var hostname = beaconMessage.PeerHost;
            var buffer = beaconMessage.Bytes;
            using (var ms = new MemoryStream(buffer))
            {
                try
                {
                    var beacon = Serializer.Deserialize<Beacon>(ms);
                    beaconList.Add(beacon, hostname);
                }
                catch (Exception)
                {
                    // Ignore stray packets
                }
            }

        }
    }
}
