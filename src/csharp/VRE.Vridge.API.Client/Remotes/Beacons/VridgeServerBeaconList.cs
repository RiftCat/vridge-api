using System;
using System.Collections.Generic;
using System.Text;
using VRE.Vridge.API.Client.Messages.v3.Discovery;

namespace VRE.Vridge.API.Client.Remotes.Beacons
{
    class VridgeServerBeaconList
    {
        private readonly Dictionary<string, VridgeServerBeacon> timedList = new Dictionary<string, VridgeServerBeacon>();

        public void Add(Beacon beacon, string endpoint)
        {
            if (timedList.ContainsKey(endpoint))
            {
                timedList.Remove(endpoint);
            }

            timedList.Add(endpoint, new VridgeServerBeacon(beacon, endpoint));
        }

        public List<VridgeServerBeacon> FreshServers
        {
            get
            {
                var beacons = new List<VridgeServerBeacon>();
                foreach (var beacon in timedList.Values)
                {
                    if (beacon.IsFresh)
                    {
                        beacons.Add(beacon);
                    }
                }

                return beacons;
            }
        }
    }
}
