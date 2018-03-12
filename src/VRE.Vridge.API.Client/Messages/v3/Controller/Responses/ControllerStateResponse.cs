using System.Runtime.InteropServices;
using ProtoBuf;

namespace VRE.Vridge.API.Client.Messages.v3.Controller.Responses
{
    [ProtoContract]
    public struct ControllerStateResponse
    {        
        [ProtoMember(1)]
        public int Version;

        [ProtoMember(2)]
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
