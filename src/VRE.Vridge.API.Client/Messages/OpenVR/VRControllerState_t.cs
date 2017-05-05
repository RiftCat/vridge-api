using System.Runtime.InteropServices;

namespace VRE.Vridge.API.Client.Messages.OpenVR
{
    /// <summary>
    /// See VRControllerState_t in OpenVR docs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 60)]
    public struct VRControllerState_t
    {
        public uint unPacketNum;
        public ulong ulButtonPressed;
        public ulong ulButtonTouched;
        public VRControllerAxis_t rAxis0;
        public VRControllerAxis_t rAxis1;
        public VRControllerAxis_t rAxis2;
        public VRControllerAxis_t rAxis3;
        public VRControllerAxis_t rAxis4;
    }
}
