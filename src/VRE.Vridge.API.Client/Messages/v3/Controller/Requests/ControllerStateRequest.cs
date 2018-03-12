using System.Runtime.InteropServices;
using ProtoBuf;
using VRE.Vridge.API.Client.Messages.v3.Controller.Responses;


namespace VRE.Vridge.API.Client.Messages.v3.Controller.Requests
{
    /// <summary>
    /// Request to a VRidge API server that contains current motion controller state.
    /// <see cref="ControllerStateResponse"/> will be returned as a response.    
    /// </summary>
    [ProtoContract]    
    public struct ControllerStateRequest 
    {
        private const int CurrentVersion = 3;

        [ProtoMember(1)]
        public int Version;

        /// <summary>
        /// Describes how API should handle the incoming data, see <see cref="ControllerTask"/>.
        /// </summary>
        [ProtoMember(2)]
        public byte TaskType;


        [ProtoMember(3)]
        public byte Origin;
        
        [ProtoMember(4)]
        public VRController ControllerState;
    }

    public enum ControllerTask
    {
        /// <summary>
        /// Packet closes your controller API connection and lets other clients use it.
        /// </summary>        
        Disconnect = 255,

        /// <summary>
        /// Packet contains full controller state as defined in <see cref="VRController"/> struct.
        /// It will be used immediately.
        /// </summary>
        SendFullState = 1,        
    }

    public enum ControllerOrigin
    {
        /// <summary>
        /// Used by controllers that use their own coordinate space. 
        /// Using Tracking API is highly recommended in this scenario.
        /// </summary>
        Zero = 0,

        // Used by controllers with tracking systems attached to head (e.g. Leap Motion, Finch Shift)
        //[NYI] Head = 1
    }

    

}
