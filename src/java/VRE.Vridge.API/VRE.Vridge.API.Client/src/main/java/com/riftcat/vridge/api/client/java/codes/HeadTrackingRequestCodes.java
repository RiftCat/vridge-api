package com.riftcat.vridge.api.client.java.codes;

public class HeadTrackingRequestCodes {

    public static int Disconnect = 255;

    public static int ChangeState = 254;
    public static int Recenter = 50;

    public static int SendPoseMatrixRotationOnly = 0;
    public static int SendPoseMatrixFull = 6;
    public static int SendRotationMatrix = 1;
    public static int SendRadRotationAndPosition = 3;
    public static int SendQuatRotationAndPosition = 4;
    public static int SendPositionOnly = 5;

    public static int RequestSyncOffset = 100;
    public static int RequestReadOnlyPose = 199;
    public static int RequestReadOnlyPhonePose = 200;

    public static int SetYawOffset = 201;
    public static int ResetYawOffset = 21;
}
