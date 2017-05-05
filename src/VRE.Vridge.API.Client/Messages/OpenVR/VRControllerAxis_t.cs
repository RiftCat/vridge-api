using System.Runtime.InteropServices;

namespace VRE.Vridge.API.Client.Messages.OpenVR
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct VRControllerAxis_t
    {
        public float x;
        public float y;

        public VRControllerAxis_t(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}