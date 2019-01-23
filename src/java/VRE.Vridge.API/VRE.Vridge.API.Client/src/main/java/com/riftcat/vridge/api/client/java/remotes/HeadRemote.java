package com.riftcat.vridge.api.client.java.remotes;

import com.riftcat.vridge.api.client.java.proto.TrackedPose;
import com.riftcat.vridge.api.client.java.proxy.HeadTrackingProxy;

import java.util.concurrent.TimeoutException;

public class HeadRemote extends RemoteBase {
    private HeadTrackingProxy proxy;

    HeadRemote(HeadTrackingProxy proxy) {
        super(proxy);

        this.proxy = proxy;
    }

    /**
     * Reorients tracking system and sets new center to current head direction.
     */
    public void recenter(){
        try{
            proxy.recenterView();
        }
        catch (Exception x){
            dispose();
        }

    }

    /**
     * Gets current head pose and related offsets. May return null on connection issues.
     */
    public TrackedPose getCurrentPose() {

        try{
            return proxy.getCurrentPose();
        }
        catch (Exception e){
            dispose();
            return null;
        }
    }

    /**
     * Sets head position to new location.
     */
    public void setPosition(float x, float y, float z){
        try{
            proxy.setPosition(x, y, z);
        }
        catch (Exception e){
            dispose();
        }
    }

    /**
     * Sets head position to new location and orientation.
     * This won't work for headsets with reprojection enabled.
     */
    public void setRotationAndPosition(float yaw, float pitch, float roll, float x, float y, float z){
        try{
            proxy.setRotationAndPosition(yaw, pitch, roll, x, y, z);
        }
        catch (Exception e){
            dispose();
        }
    }


    /**
     * Sets offsets in yaw axis to be applied to each head and controller pose processed by the system.
     * @param yaw Offset in radians.
     */
    public void setYawOffset(float yaw){
        try{
            proxy.setYawOffset(yaw);
        }
        catch (Exception x){
            dispose();
        }
    }

    /**
     * Marks the headset as in/outside of tracking range. Setting it to false will most likely
     * stop rendering on SteamVR side as pose data will be considered invalid.
     */
    public void setStatus(boolean isInTrackingRange)
    {
        try{
            proxy.changeTrackingState(isInTrackingRange);
        }
        catch (Exception e){
            dispose();
        }
    }

    // Type-cast-methods
    /**
     * Sets head position to new location and orientation.
     * This won't work for headsets with reprojection enabled.
     */
    public void setRotationAndPosition(double yaw, double pitch, double roll, double x, double y, double z){
        setRotationAndPosition((float) yaw, (float) pitch, (float) roll, (float) x, (float) y, (float) z);
    }

    /**
     * Sets head position to new location.
     */
    public void setPosition(double x, double y, double z) {
        setPosition((float) x, (float) y, (float) z);
    }
}
