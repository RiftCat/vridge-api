using System;
using System.Runtime.InteropServices;
using ProtoBuf;
using VRE.Vridge.API.Client.Messages.BasicTypes;
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

        [ProtoMember(9)]
        public double Timestamp;

        [ProtoMember(3), Obsolete("Use Position + Orientation instead.")]
        public float[] OrientationMatrix;

        [ProtoMember(4)]
        public VRControllerState_t ButtonState;

        [ProtoMember(5)]
        public double[] Acceleration;

        [ProtoMember(6)]
        public double[] Velocity;

        [ProtoMember(7)] // TODO: GitHub API docs
        public HeadRelation HeadRelation;

        
        [ProtoMember(8)] // TODO: GitHub API docs
        public HandType SuggestedHand;    

        [ProtoMember(11)]
        public string Name;

        /// <summary>
        /// XYZ vector.
        /// </summary>
        [ProtoMember(12)]
        public float[] Position;


        /// <summary>
        /// XYZW quaternion.
        /// </summary>
        [ProtoMember(13)]
        public float[] Orientation;
        
        // [ProtoMember(14)]

        public bool ShouldRemap3To6Dof => OrientationMatrix == null && Position == null;

    }
}
