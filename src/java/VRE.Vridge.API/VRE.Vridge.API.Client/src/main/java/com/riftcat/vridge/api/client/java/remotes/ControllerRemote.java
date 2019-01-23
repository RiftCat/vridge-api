package com.riftcat.vridge.api.client.java.remotes;

import com.riftcat.vridge.api.client.java.proto.HandType;
import com.riftcat.vridge.api.client.java.proto.HeadRelation;
import com.riftcat.vridge.api.client.java.proto.VRController;
import com.riftcat.vridge.api.client.java.proto.VRControllerAxis_t;
import com.riftcat.vridge.api.client.java.proto.VRControllerState_t;
import com.riftcat.vridge.api.client.java.proto.VRControllerState_tOrBuilder;
import com.riftcat.vridge.api.client.java.proxy.ControllerProxy;
import com.riftcat.vridge.api.client.java.utils.ButtonMask;

import java.util.concurrent.TimeoutException;

public class ControllerRemote extends RemoteBase {

    private static int packetNum = 0;
    private ControllerProxy proxy;

    ControllerRemote(ControllerProxy proxy) {
        super(proxy);
        this.proxy = proxy;
    }

    /**
     * Sets VR controller to a new state
     * @param controllerId Unique ID of given controller
     * @param headRelation How given pose relates to the head. Usually unrelated is the best pick.
     * @param orientation Orientation as XYZW float[4].
     * @param position Position as XYZ float[3].
     * @param analogX (-1,1) horizontal touchpad position.
     * @param analogY (-1,1) vertical touchpad position.
     * @param analogTrigger (0,1) trigger state (1 is fully pulled)
     * @param isMenuPressed
     * @param isSystemPressed
     * @param isTriggerPressed
     * @param isGripPressed
     * @param isTouchpadPressed
     * @param isTouchpadTouched
     */
    public void setControllerState(
            // Controller ID
            int controllerId,
            HandType handType,
            boolean disableController,
            // Pose data
            HeadRelation headRelation,
            float[] orientation,
            float[] position,
            // Touchpad state [-1,1]
            double analogX,
            double analogY,

            // Trigger state
            double analogTrigger,

            // Button states
            boolean isMenuPressed,
            boolean isSystemPressed,
            boolean isTriggerPressed,
            boolean isGripPressed,
            boolean isTouchpadPressed,
            boolean isTouchpadTouched){

        // See openvr.h in OpenVR SDK for mappings and masks
        // https://github.com/ValveSoftware/openvr/blob/master/headers/openvr.h

        long pressedMask = buildPressedMask(isMenuPressed, isSystemPressed, isTriggerPressed, isGripPressed, isTouchpadPressed);
        long touchedMask = buildTouchedMask(isTouchpadTouched, isTriggerPressed);

        int controllerStatus = 0;
        if(disableController == true) {
            controllerStatus = 2;
        }

        VRControllerState_tOrBuilder buttons = VRControllerState_t.newBuilder()
                .setRAxis0(VRControllerAxis_t.newBuilder()
                        .setX((float) analogX)
                        .setY((float) analogY))
                .setRAxis1(VRControllerAxis_t.newBuilder()
                        .setX((float) analogTrigger))
                .setUlButtonPressed(pressedMask)
                .setUlButtonTouched(touchedMask)
                .setUnPacketNum(++packetNum);      // Touchpad


        VRController.Builder controllerState = VRController.newBuilder()
                .setControllerId(controllerId)
                .addOrientation(orientation[0])
                .addOrientation(orientation[1])
                .addOrientation(orientation[2])
                .addOrientation(orientation[3])
                .setStatus(controllerStatus)
                .setSuggestedHand(handType)
                .setHeadRelation(headRelation)
                .setButtonState((VRControllerState_t.Builder) buttons);

        if(position != null){
            controllerState.addPosition(position[0]);
            controllerState.addPosition(position[1]);
            controllerState.addPosition(position[2]);
        }

        try {
            proxy.sendControllerState(controllerState.build());
        } catch (Exception e) {
            dispose();
        }
    }

    /** Recenter head tracking. Works the same as pressing recenter hotkey as configured in VRidge settings. */
    public void recenterHead(){
        try{
            proxy.recenterHead();
        }
        catch (Exception e){
            dispose();
        }
    }

    private long buildTouchedMask(boolean isTouchpadTouched, boolean isTriggerTouched) {
        long mask = 0;
        if (isTouchpadTouched) mask |= ButtonMask.Touchpad;
        if (isTriggerTouched)  mask |= ButtonMask.Trigger;
        return mask;
    }

    private long buildPressedMask(
            boolean isMenuPressed,
            boolean isSystemPressed,
            boolean isTriggerPressed,
            boolean isGripPressed,
            boolean isTouchpadPressed) {

        long mask = 0;
        if (isMenuPressed) mask |= ButtonMask.ApplicationMenu;
        if (isSystemPressed) mask |= ButtonMask.System;
        if (isTriggerPressed) mask |= ButtonMask.Trigger;
        if (isGripPressed) mask |= ButtonMask.Grip;
        if (isTouchpadPressed) mask |= ButtonMask.Touchpad;
        return mask;
    }
}
