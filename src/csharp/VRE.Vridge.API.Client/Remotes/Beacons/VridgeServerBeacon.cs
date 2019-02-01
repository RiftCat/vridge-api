using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VRE.Vridge.API.Client.Messages.v3.Discovery;

namespace VRE.Vridge.API.Client.Remotes.Beacons
{
    public class VridgeServerBeacon
    {
        private const long TimeoutMs = 5000;
        private static readonly Stopwatch Timer = Stopwatch.StartNew();

        private readonly Beacon beacon;
        private readonly string endpoint;
        private readonly long timestmapMs;        

        public VridgeServerBeacon(Beacon beacon, string endpoint)
        {
            this.beacon = beacon;
            this.endpoint = endpoint;
            timestmapMs = Timer.ElapsedMilliseconds;
        }

        public Beacon Beacon => beacon;
        public string Endpoint => endpoint;
        public bool IsFresh => timestmapMs + TimeoutMs > Timer.ElapsedMilliseconds;

        public override string ToString()
        {
            return beacon.Role + "|" +
                   beacon.HumanReadableVersion + "|" +
                   beacon.MachineName + "|" +
                   beacon.UserName + "@" +
                   endpoint;
        }
    }
}
