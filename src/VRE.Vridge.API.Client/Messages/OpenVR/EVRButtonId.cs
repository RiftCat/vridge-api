namespace VRE.Vridge.API.Client.Messages.OpenVR
{
    public enum EVRButtonId
    {
        k_EButton_System = 0,           //  0
        k_EButton_ApplicationMenu = 1,  //  1
        k_EButton_Grip = 2,             // 01
        k_EButton_DPad_Left = 3,        // 11
        k_EButton_DPad_Up = 4,
        k_EButton_DPad_Right = 5,
        k_EButton_DPad_Down = 6,
        k_EButton_A = 7,

        k_EButton_ProximitySensor = 31,

        k_EButton_Axis0 = 32,
        k_EButton_Axis1 = 33,
        k_EButton_Axis2 = 34,
        k_EButton_Axis3 = 35,
        k_EButton_Axis4 = 36,

        // aliases for well known controllers
        k_EButton_SteamVR_Touchpad = k_EButton_Axis0,
        k_EButton_SteamVR_Trigger = k_EButton_Axis1,

        k_EButton_Dashboard_Back = k_EButton_Grip,

        k_EButton_Max = 64
    };

    public class ButtonMask
    {
        public const ulong System = (1ul << (int)EVRButtonId.k_EButton_System);
        public const ulong ApplicationMenu = (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu);
        public const ulong Grip = (1ul << (int)EVRButtonId.k_EButton_Grip);
        public const ulong Axis0 = (1ul << (int)EVRButtonId.k_EButton_Axis0);
        public const ulong Axis1 = (1ul << (int)EVRButtonId.k_EButton_Axis1);
        public const ulong Axis2 = (1ul << (int)EVRButtonId.k_EButton_Axis2);
        public const ulong Axis3 = (1ul << (int)EVRButtonId.k_EButton_Axis3);
        public const ulong Axis4 = (1ul << (int)EVRButtonId.k_EButton_Axis4);
        public const ulong Touchpad = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad);
        public const ulong Trigger = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger);
    }
}
