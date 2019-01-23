using System.Runtime.InteropServices;

namespace VRE.Vridge.API.Client.Messages.OpenVR
{
    [StructLayout(LayoutKind.Sequential, Size = 196)]
    public struct VRController
    {
        public int ControllerId;

        public int Status;

        public double Timestamp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] OrientationMatrix;

        public VRControllerState_t ButtonState;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] Acceleration;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] Velocity;

        //0-undefined, 1-left, 2-right
        public int Role;

        public double PoseTimeOffset;
    }
}
