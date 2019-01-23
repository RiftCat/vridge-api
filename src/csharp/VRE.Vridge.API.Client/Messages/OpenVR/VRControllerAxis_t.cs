using System.Runtime.InteropServices;
using ProtoBuf;

namespace VRE.Vridge.API.Client.Messages.OpenVR
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    [ProtoContract]
    public struct VRControllerAxis_t
    {
        [ProtoMember(1)]
        public float x;

        [ProtoMember(2)]
        public float y;

        public VRControllerAxis_t(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}