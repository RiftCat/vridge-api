using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace VRE.Vridge.API.Client.Messages.v3.Discovery
{
    [ProtoContract]
    public class Beacon
    {
        [ProtoMember(1)]
        public BeaconOrigin Role;

        [ProtoMember(2)]
        public string MachineName { get; set; }

        [ProtoMember(3)]
        public string UserName { get; set; }

        [ProtoMember(4)]
        public string HumanReadableVersion { get; set; }
    }
}
