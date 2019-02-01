package com.riftcat.vridge.api.client.java.proxy;

import com.riftcat.vridge.api.client.java.codes.ControllerStateRequestCodes;
import com.riftcat.vridge.api.client.java.proto.ControllerStateRequest;
import com.riftcat.vridge.api.client.java.proto.ControllerStateResponse;
import com.riftcat.vridge.api.client.java.proto.HandType;
import com.riftcat.vridge.api.client.java.proto.VRController;
import com.riftcat.vridge.api.client.java.proto.VRControllerAxis_t;
import com.riftcat.vridge.api.client.java.proto.VRControllerState_t;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.TimeoutException;

public class ControllerProxy extends ClientProxyBase {

    private int packetNum = 0;

    public ControllerProxy(String endpointAddress){
        super(endpointAddress, true);
    }

    /// <summary>
    /// Send full single VR controller state to VR.
    /// </summary>
    public synchronized void sendControllerState(VRController state) throws TimeoutException {
        ControllerStateRequest data = ControllerStateRequest
                .newBuilder()
                .setTaskType(ControllerStateRequestCodes.SendFullState)
                .setControllerState(state)
                .build();

        sendMessage(data);
    }

    /**
     * Send full single VR controller state to VR.
     */
    public synchronized void sendControllerState(int controllerId, long touchedMask, long pressedMask,
                                                 List<Float> orientationMatrix,
                                                 float triggerValue,
                                                 float analogX, float analogY, float[] velocity,
                                                 HandType hand) throws TimeoutException {

        VRControllerState_t.Builder buttonState = VRControllerState_t.newBuilder()
                .setRAxis0(VRControllerAxis_t.newBuilder()
                        .setX(analogX)
                        .setY(analogY))
                .setRAxis1(VRControllerAxis_t.newBuilder()
                        .setX(triggerValue))
                .setUlButtonPressed(pressedMask)
                .setUlButtonTouched(touchedMask)
                .setUnPacketNum(++packetNum);



        VRController.Builder controllerState = VRController.newBuilder()
                .setControllerId(controllerId)
                .addAllOrientationMatrix(orientationMatrix)
                .setStatus(0)
                .setSuggestedHand(hand)
                .setButtonState(buttonState);

        if(velocity != null){
            controllerState
                    .addVelocity(velocity[0])
                    .addVelocity(velocity[1])
                    .addVelocity(velocity[2]);
        }

        ControllerStateRequest request = ControllerStateRequest.newBuilder()
                .setTaskType(ControllerStateRequestCodes.SendFullState)
                .setControllerState(controllerState)
                .build();

        sendMessage(request);
    }

    /** Recenter head tracking. Works the same as pressing recenter hotkey as configured in VRidge settings. */
    public void recenterHead() throws TimeoutException{
        ControllerStateRequest request = ControllerStateRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(ControllerStateRequestCodes.RecenterHead)
                .build();
        sendMessage(request);
    }

    /**
     * Disconnected from controller API and frees the API for other clients.
     */
    public void disconnect(){
        ControllerStateRequest disconnectRequest = ControllerStateRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(ControllerStateRequestCodes.Disconnect)
                .build();

        try{
            sendMessage(disconnectRequest);
        }
        catch (Exception x){
            // ignored
        }
        CloseSocket();
    }

        private ControllerStateResponse sendMessage(ControllerStateRequest req) throws TimeoutException {
        return SendMessage(req, ControllerStateResponse.parser());
    }
}
