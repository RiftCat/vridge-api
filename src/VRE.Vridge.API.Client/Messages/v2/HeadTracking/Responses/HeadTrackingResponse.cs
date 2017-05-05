using System.Runtime.InteropServices;

namespace VRE.Vridge.API.Client.Messages.v2.HeadTracking.Responses
{
    /// <summary>
    /// Upgrade note v1->v2: No changes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct HeadTrackingResponse
    {
        public int Version;
        public byte ReplyCode;      
        public int DataLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] Data;
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
            /// Data contains float[6] (24 bytes total):
            /// pitch(+up), yaw (+to left), roll(+left), X, Y, Z 
            /// X Y Z position is always zero because no phone-side positional 
            /// tracking exists currently. This may change in the future.
            /// </summary>
            SendingCurrentPhonePose = 1,
        }
    }
}
