using System.Runtime.InteropServices;
using VRE.Vridge.API.Client.Messages.v2.Controller.Responses;


namespace VRE.Vridge.API.Client.Messages.v2.Controller.Requests
{
    /// <summary>
    /// Request to a VRidge API server that contains current motion controller state.
    /// <see cref="ControllerStateResponse"/> will be returned as a response.
    /// 
    /// Upgrade note v1->v2: No changes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ControllerRequest 
    {
        private const int CurrentVersion = 2;

        public int Version;

        /// <summary>
        /// Describes how API should handle the incoming data, see <see cref="ControllerTask"/>.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public byte TaskType;


        [MarshalAs(UnmanagedType.I1)]
        public byte Origin;
                
        // Upgrade note v1->v2: No changes in VRController format. We can use the same layout.
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
