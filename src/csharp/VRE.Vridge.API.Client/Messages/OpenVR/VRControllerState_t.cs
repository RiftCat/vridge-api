using System.Runtime.InteropServices;
using ProtoBuf;

namespace VRE.Vridge.API.Client.Messages.OpenVR
{
    /// <summary>
    /// See VRControllerState_t in OpenVR docs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 60)]
    [ProtoContract]
    public struct VRControllerState_t
    {
        [ProtoMember(1)]
        public uint unPacketNum;

        [ProtoMember(2)]
        public ulong ulButtonPressed;

        [ProtoMember(3)]
        public ulong ulButtonTouched;

        [ProtoMember(4)]
        public VRControllerAxis_t rAxis0;

        [ProtoMember(5)]
        public VRControllerAxis_t rAxis1;

        [ProtoMember(7)]
        public VRControllerAxis_t rAxis2;

        [ProtoMember(8)]
        public VRControllerAxis_t rAxis3;

        [ProtoMember(9)]
        public VRControllerAxis_t rAxis4;
    }
}
