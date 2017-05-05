using System.Runtime.InteropServices;
using VRE.Vridge.API.Client.Messages.OpenVR;

namespace VRE.Vridge.API.Client.Messages.v2.Controller
{
    [StructLayout(LayoutKind.Sequential, Size = 132)]
    public struct VRController
    {
        public int ControllerId;


        /// <summary>
        /// <see cref="TrackedDeviceStatus"/>
        /// </summary>
        public int Status;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] OrientationMatrix;

        public VRControllerState_t ButtonState;
    }
}
