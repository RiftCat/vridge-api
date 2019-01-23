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
        public const int CurrentVersion = 3;

        [ProtoMember(1)]
        public int Version;

        /// <summary>
        /// Describes how API should handle the incoming data, see <see cref="ControllerTask"/>.
        /// </summary>
        [ProtoMember(2)]
        public byte TaskType;


        // Origin is removed, was never used, see VRController.HeadRelation for replacement
        // [ProtoMember(3)] public byte Origin;


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

        /// <summary>
        /// Recenter head tracking. Works the same as pressing recenter hotkey as configured in VRidge settings.
        /// </summary>
        RecenterHead = 2
    }  
}
