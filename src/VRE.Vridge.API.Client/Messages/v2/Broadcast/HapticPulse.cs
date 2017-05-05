using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VRE.Vridge.API.Client.Messages.v2.Broadcast
{    
    [StructLayout(LayoutKind.Sequential)] // Default field packing == 8
    public struct HapticPulse
    {
        /// <summary>
        /// Udentifier defined by controller when it sends its requests to OpenVR.
        /// </summary>
        public int ControllerId;


        /// <summary>
        /// Duration of pulse in microseconds, provided by VR game.
        /// </summary>
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
        public uint TimestampUs;
    }
}
