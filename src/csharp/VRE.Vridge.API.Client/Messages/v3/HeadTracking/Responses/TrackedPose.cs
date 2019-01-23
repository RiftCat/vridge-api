using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace VRE.Vridge.API.Client.Messages.v3.HeadTracking.Responses
{
    [ProtoContract]
    public class TrackedPose
    {
        /// <summary>
        /// Current head orientation as 4 element XYZW quaternion.
        /// </summary>
        [ProtoMember(1)]
        public float[] HeadOrientation;

        /// <summary>
        /// Current head position as 3 element XYZ vector.
        /// </summary>
        [ProtoMember(2)]
        public float[] HeadPosition;

        /// <summary>
        /// Current offset applied to each head-related (Controller w/ HeadRelation.IsInHeadSpace or internal VRidge mobile tracking data) pose due to user recenter.
        /// In 99% cases, you can forget about it.
        /// </summary>
        [ProtoMember(3)]
        public float RecenterYawOffset;

        /// <summary>
        /// Current offset applied to each pose due to API SetYawOffset call.
        /// </summary>
        [ProtoMember(4)]
        public float ApiYawOffset;

    }
}
