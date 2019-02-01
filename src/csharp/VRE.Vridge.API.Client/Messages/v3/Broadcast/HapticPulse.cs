using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace VRE.Vridge.API.Client.Messages.v3.Broadcast
{    
    [ProtoContract]
    public struct HapticPulse
    {
        /// <summary>
        /// Identifier defined by controller when it sends its requests to OpenVR.
        /// </summary>
        [ProtoMember(1)]
        public int ControllerId;


        /// <summary>
        /// Duration of pulse in microseconds, provided by VR game.
        /// </summary>
        [ProtoMember(2)]
        public uint LengthUs;

        /// <summary>
        /// High resolution (1us) timestamp in microseconds - based on QueryPerformanceCounter().
        /// Can be optionally used to smooth out or join pulses together.
        /// 
        /// <remarks>
        /// Timestamp is created when the pulse was submitted to HMD driver.
        /// Wrap-arounds will happen so use it for interval measurement, not absolute timings. 
        /// </remarks>
        /// </summary>
        [ProtoMember(3)]
        public uint TimestampUs;
    }
}
