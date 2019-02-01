package com.riftcat.vridge.api.client.java.utils;

public class ButtonMask{
    private static int k_EButton_System = 0;
    private static int k_EButton_ApplicationMenu = 1;
    private static int k_EButton_Grip = 2;
    private static int k_EButton_DPad_Left = 3;
    private static int k_EButton_DPad_Up = 4;
    private static int k_EButton_DPad_Right = 5;
    private static int k_EButton_DPad_Down = 6;
    private static int k_EButton_A = 7;

    private static int k_EButton_ProximitySensor = 31;

    private static int k_EButton_Axis0 = 32;
    private static int k_EButton_Axis1 = 33;
    private static int k_EButton_Axis2 = 34;
    private static int k_EButton_Axis3 = 35;
    private static int k_EButton_Axis4 = 36;

    // aliases for well known controllers
    private static int k_EButton_SteamVR_Touchpad = k_EButton_Axis0;
    private static int k_EButton_SteamVR_Trigger = k_EButton_Axis1;
    private static int k_EButton_Dashboard_Back = k_EButton_Grip;
    private static int k_EButton_Max = 64;

    public static long System = (1L << (int)k_EButton_System);
    public static long ApplicationMenu = (1L << (int)k_EButton_ApplicationMenu);
    public static long Grip = (1L << (int)k_EButton_Grip);
    public static long Axis0 = (1L << (int)k_EButton_Axis0);
    public static long Axis1 = (1L << (int)k_EButton_Axis1);
    public static long Axis2 = (1L << (int)k_EButton_Axis2);
    public static long Axis3 = (1L << (int)k_EButton_Axis3);
    public static long Axis4 = (1L << (int)k_EButton_Axis4);
    public static long Touchpad = (1L << (int)k_EButton_SteamVR_Touchpad);
    public static long Trigger = (1L << (int)k_EButton_SteamVR_Trigger);
}
