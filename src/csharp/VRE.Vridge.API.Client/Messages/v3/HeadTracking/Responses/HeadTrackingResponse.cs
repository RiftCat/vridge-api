using System;
using System.Runtime.InteropServices;
using ProtoBuf;

namespace VRE.Vridge.API.Client.Messages.v3.HeadTracking.Responses
{        
    [ProtoContract]
    public struct HeadTrackingResponse
    {
        [ProtoMember(1)]
        public int Version;

        [ProtoMember(2)]
        public byte ReplyCode;

        [ProtoMember(3)]
        public float[] Data;

        [ProtoMember(4)]
        public TrackedPose TrackedPose;

        public enum Response
        {
            /// <summary>
            /// In response to disconnect request.
            /// </summary>
            Disconnecting = 255,

            /// <summary>
            /// When request was not understood
            /// </summary>
            BadRequest = 254,

            /// <summary>
            /// No new data was received from the phone for 5 seconds, possibly phone lost connection
            /// </summary>
            PhoneDataTimeout = 253,


            AcceptedYourData = 0,

            /// <summary>
            /// Pose is contained in TrackedPose field.
            /// </summary>
            SendingCurrentTrackedPose = 2,

            /// <summary>
            /// Data contains float[6] (24 bytes total):
            /// pitch(+up), yaw (+to left), roll(+left), X, Y, Z 
            /// X Y Z position is always zero because no phone-side positional 
            /// tracking exists currently. This may change in the future.
            /// </summary>
            [Obsolete]
            SendingCurrentPhonePose = 1,
        }
    }
}
