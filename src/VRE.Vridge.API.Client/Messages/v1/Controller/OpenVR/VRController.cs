using System.Runtime.InteropServices;

namespace VRE.Vridge.API.Client.Messages.v1.Controller.OpenVR
{
[StructLayout(LayoutKind.Sequential, Size = 132)]
public struct VRController
{
    public int ControllerId;
    public int Status;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public float[] OrientationMatrix;

    public VRControllerState_t ButtonState;
}

    internal enum VRControllerStatus
    {        
        Active = 0,
        TempUnavailable = 1,
        Disabled = 2
    }
}
