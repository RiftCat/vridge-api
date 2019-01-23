package com.riftcat.vridge.api.client.java.proxy;

import com.google.protobuf.ByteString;
import com.riftcat.vridge.api.client.java.codes.HeadTrackingRequestCodes;
import com.riftcat.vridge.api.client.java.codes.HeadTrackingResponseCodes;
import com.riftcat.vridge.api.client.java.codes.TrackedDeviceStatus;
import com.riftcat.vridge.api.client.java.proto.HeadTrackingRequest;
import com.riftcat.vridge.api.client.java.proto.HeadTrackingResponse;
import com.riftcat.vridge.api.client.java.proto.TrackedPose;
import com.riftcat.vridge.api.client.java.utils.APILogger;
import com.riftcat.vridge.api.client.java.utils.SerializationUtils;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.concurrent.TimeoutException;

public class HeadTrackingProxy extends ClientProxyBase {

    // This is only partial implementation of API calls

    public HeadTrackingProxy(String endpointAddress, boolean shouldKeepAlive){
        super(endpointAddress, shouldKeepAlive);
    }

    /**
     * Sets head position to new location.
     */
    public boolean setPosition(float x, float y, float z) throws TimeoutException {
        HeadTrackingRequest request = HeadTrackingRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.SendPositionOnly)
                .setData(SerializationUtils.byteStringFromFloats(x, y, z))
                .build();

        HeadTrackingResponse reply = sendMessage(request);

        return reply.getReplyCode() == HeadTrackingResponseCodes.AcceptedYourData;
    }

    /**
     Sets position and rotation and returns true if the value was accepted.
     This won't work for headsets with reprojection enabled.
     */
    public boolean setRotationAndPosition(float yaw, float pitch, float roll, float x, float y, float z) throws TimeoutException {
        HeadTrackingRequest request = HeadTrackingRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.SendRadRotationAndPosition)
                .setData(SerializationUtils.byteStringFromFloats(pitch, yaw, roll, x, y, z))
                .build();

        HeadTrackingResponse reply = sendMessage(request);

        return reply.getReplyCode() == HeadTrackingResponseCodes.AcceptedYourData;
    }

    /**
     Sets position and rotation and returns true if the value was accepted.
     This won't work for headsets with reprojection enabled.
     */
    public boolean setRotationAndPosition(float quatQ, float quatY, float quatZ, float quatW, float posX, float posY, float posZ) throws TimeoutException {
        HeadTrackingRequest request = HeadTrackingRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.SendQuatRotationAndPosition)
                .setData(SerializationUtils.byteStringFromFloats(quatQ, quatY, quatZ, quatW, posX, posY, posZ))
                .build();

        HeadTrackingResponse reply = sendMessage(request);

        return reply.getReplyCode() == HeadTrackingResponseCodes.AcceptedYourData;
    }

    /**
     * Reorients tracking system and sets new center to current head direction.
     */
    public boolean recenterView() throws TimeoutException {
        HeadTrackingRequest request = HeadTrackingRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.Recenter)
                .build();

        HeadTrackingResponse reply = sendMessage(request);

        return reply.getReplyCode() == HeadTrackingResponseCodes.AcceptedYourData;
    }


    /**
     * Gets current head pose and related offsets.
     */
    public TrackedPose getCurrentPose() throws TimeoutException {

        HeadTrackingRequest request = HeadTrackingRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.RequestReadOnlyPose)
                .build();

        HeadTrackingResponse reply = sendMessage(request);

        if(reply.getReplyCode() == HeadTrackingResponseCodes.SendingCurrentTrackedPose){
            return reply.getTrackedPose();
        }
        else{
            return null;
        }
    }

    public void setYawOffset(float yaw) throws TimeoutException {
        HeadTrackingRequest request = HeadTrackingRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.SetYawOffset)
                .setData(SerializationUtils.byteStringFromFloats(yaw))
                .build();

        sendMessage(request);
    }

    public void changeTrackingState(boolean isInTrackingRange) throws TimeoutException {
        byte[] state = new byte[1];
        state[0] = isInTrackingRange ? TrackedDeviceStatus.Active : TrackedDeviceStatus.TempUnavailable;
        HeadTrackingRequest request = HeadTrackingRequest
                .newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.ChangeState)
                .setData(ByteString.copyFrom(state))
                .build();

        sendMessage(request);
    }

    @Override
    public void disconnect() {
        HeadTrackingRequest disconnectRequest = HeadTrackingRequest.newBuilder()
                .setVersion(CurrentVersion)
                .setTaskType(HeadTrackingRequestCodes.Disconnect)
                .build();

        try{
            sendMessage(disconnectRequest);
        }
        catch (Exception x){
            // ignored
        }

        CloseSocket();
    }

    private HeadTrackingResponse sendMessage(HeadTrackingRequest req) throws TimeoutException {
        return SendMessage(req, HeadTrackingResponse.parser());
    }
}
