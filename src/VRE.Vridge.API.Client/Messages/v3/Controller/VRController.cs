using System.Runtime.InteropServices;
using ProtoBuf;
using VRE.Vridge.API.Client.Messages.OpenVR;

namespace VRE.Vridge.API.Client.Messages.v3.Controller
{
    [ProtoContract]
    public struct VRController
    {
        [ProtoMember(1)]
        public int ControllerId;


        /// <summary>
        /// <see cref="TrackedDeviceStatus"/>
        /// </summary>
        [ProtoMember(2)]
        public int Status;
        
        [ProtoMember(3)]
        public float[] OrientationMatrix;

        [ProtoMember(4)]
        public VRControllerState_t ButtonState;

        [ProtoMember(5)]
        public double[] Acceleration;

        [ProtoMember(6)]
        public double[] Velocity;
    }
}
