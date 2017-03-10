using System.Runtime.InteropServices;

namespace VRE.Vridge.API.Client.Messages.v1.Controller.Responses
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ControllerStateResponse
    {        
        public int Version;
        public byte ReplyCode;

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

            AcceptedYourData = 0
        }        
    }    
}
