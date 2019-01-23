package com.riftcat.vridge.api.client.java.remotes;

import com.google.protobuf.InvalidProtocolBufferException;
import com.riftcat.vridge.api.client.java.APIClient;
import com.riftcat.vridge.api.client.java.Capabilities;
import com.riftcat.vridge.api.client.java.EndpointNames;
import com.riftcat.vridge.api.client.java.control.ControlResponseCode;
import com.riftcat.vridge.api.client.java.control.responses.APIStatus;
import com.riftcat.vridge.api.client.java.control.responses.EndpointStatus;
import com.riftcat.vridge.api.client.java.proto.Beacon;
import com.riftcat.vridge.api.client.java.proto.BeaconOrigin;
import com.riftcat.vridge.api.client.java.proto.HapticPulse;
import com.riftcat.vridge.api.client.java.proxy.BroadcastProxy;
import com.riftcat.vridge.api.client.java.proxy.ControllerProxy;
import com.riftcat.vridge.api.client.java.proxy.HeadTrackingProxy;
import com.riftcat.vridge.api.client.java.proxy.IBroadcastListener;
import com.riftcat.vridge.api.client.java.remotes.beacons.VridgeServerBeacon;
import com.riftcat.vridge.api.client.java.remotes.beacons.VridgeServerBeaconList;
import com.riftcat.vridge.api.client.java.utils.APILogger;

import org.zeromq.ZBeacon;

import java.net.InetAddress;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

public class VridgeRemote {

    private static DiscoveryClient discovery;

    // User config
    private final int reconnectFrequencyMs;
    private final int timeoutThresholdMs;
    private final EnumSet<Capabilities> capabilities;
    private final String appName;

    // Access objects
    private APIClient api;
    private ControllerRemote controller;
    private HeadRemote head;
    private BroadcastProxy broadcasts;

    // State
    private long lastConnectionAttempt;
    private boolean isDisposed = false;
    private List<IBroadcastListener> eventListeners = new ArrayList<IBroadcastListener>();

    static{
        discovery = new DiscoveryClient();
    }

    public VridgeRemote(
            String serverIp,
            String appName,
            EnumSet<Capabilities> capabilities,
            int reconnectFrequencyMs,
            int timeoutThresholdMs)
    {
        this.reconnectFrequencyMs = reconnectFrequencyMs;
        this.timeoutThresholdMs = timeoutThresholdMs;
        this.capabilities = capabilities;
        this.appName = appName;
        api = new APIClient(serverIp, appName);
    }

    /**
     * Returns list of currently running API servers in reachable networks
     * along with their IP addresses and identification information.
     */
    public static List<VridgeServerBeacon> getVridgeServers(){
        return discovery.getFreshServers();
    }

    /** Adds subscriber that will receive haptic feedback events from VRidge.
     * Possibly more in the future. */
    public void addEventListener(IBroadcastListener listener){
        eventListeners.add(listener);
    }

    /** Removes event listener added by addEventListener. */
    public void removeEventListener(IBroadcastListener listener){
        eventListeners.remove(listener);
    }

    public synchronized APIStatus getStatus(){
        try {
            return api.GetStatus();
        } catch (Exception e) {
            return null;
        }
    }

    /**
     * Returns controller remote if connection seems valid. Will return null if connection is not in healthy state.
     * Always check for null as a indicator of connection state.
     */
    public ControllerRemote getController()
    {
        if (isDisposed){
            throw new IllegalStateException("You already disposed this remote. Create new one.");
        }

        synchronized (this)
        {
            if (controller != null && !controller.isDisposed())
            {
                return controller;
            }

            if (lastConnectionAttempt + reconnectFrequencyMs < System.currentTimeMillis())
            {
                return trySettingUpConnection() ? controller : null;
            }

            return null;
        }
    }

    /**
     * Returns head remote if connection seems valid. Will return null if connection is not in healthy state.
     * Always check for null as a indicator of connection state.
     */
    public HeadRemote getHead()
    {
        if (isDisposed){
            throw new IllegalStateException("You already disposed this remote. Create new one.");
        }

        synchronized (this)
        {
            if (head != null && !head.isDisposed())
            {
                return head;
            }

            if (lastConnectionAttempt + reconnectFrequencyMs < System.currentTimeMillis())
            {
                return trySettingUpConnection() ? head : null;
            }

            return null;
        }
    }

    /**  Checks if current instance of VRidgeRemote can communicate with API
     * without establishing permanent connection.
     * Will timeout after 1 second without response. */
    public VridgeRemoteConnectionStatus preCheck(){
        synchronized (this){
            try{
                // First, check if we can even query status
                APIStatus status = api.GetStatus();
                if (status == null){
                    return VridgeRemoteConnectionStatus.Unreachable;
                }

                // Then verify if headTracking is available if we want it
                if (capabilities.contains(Capabilities.HeadTracking)){
                    EndpointStatus endpoint = status.findEndpoint(EndpointNames.HeadTracking);

                    if (endpoint == null){
                        // This shouldn't happen, ever
                        return VridgeRemoteConnectionStatus.UnexpectedError;
                    }

                    if (endpoint.Code == ControlResponseCode.InUse){
                        return VridgeRemoteConnectionStatus.InUse;
                    }
                }

                // And do the same for controllers; this can be refactored (DRY)
                if (capabilities.contains(Capabilities.Controllers)){
                    EndpointStatus endpoint = status.findEndpoint(EndpointNames.Controller);
                    if (endpoint == null){
                        return VridgeRemoteConnectionStatus.UnexpectedError;
                    }

                    if (endpoint.Code == ControlResponseCode.InUse){
                        return VridgeRemoteConnectionStatus.InUse;
                    }
                }

                return VridgeRemoteConnectionStatus.Okay;
            }
            catch (Exception e){
                // Discard it and return unknown error.
                // API clients can use lower level access to read actual errors instead of VRidge remote which is fire-and-forget api.
                return VridgeRemoteConnectionStatus.UnexpectedError;
            }

        }
    }

    /** Clears all active connections. This object cannot be used after disposing */
    public void dispose(){
        synchronized (this){
            isDisposed = true;
            eventListeners.clear();
            disconnectAllEndpoints();
            discovery.dispose();
        }
    }

    /** Sets up connection and returns true if all links are established. */
    private boolean trySettingUpConnection()
    {
        lastConnectionAttempt = System.currentTimeMillis();

        // Make sure API server is alive
        APIStatus status;
        try{
            status = api.GetStatus();
        }
        catch (Exception x){
            return false;
        }

        // Reset current connections, if exist
        disconnectAllEndpoints();

        try
        {
            if (capabilities.contains(Capabilities.HeadTracking))
            {
                EndpointStatus endpointStatus = status.findEndpoint(EndpointNames.HeadTracking);
                if (endpointStatus == null || endpointStatus.Code != ControlResponseCode.OK)
                {
                    return false;
                }

                head = new HeadRemote((HeadTrackingProxy)api.getProxy(APIClient.HEADTRACKING));
            }

            if (capabilities.contains(Capabilities.Controllers))
            {
                EndpointStatus endpointStatus = status.findEndpoint(EndpointNames.Controller);
                if (endpointStatus == null){
                    return false;
                }

                controller = new ControllerRemote((ControllerProxy) api.getProxy(APIClient.CONTROLLER));
            }

            // Subscribe to haptic pulse broadcasts, if controller proxy is in use
            if (controller != null){
                broadcasts = api.getProxy(APIClient.BROADCASTS);

                // Forward the events to long-lived event so API user doesn't
                // have to resubscribe on reconnect
                broadcasts.addListener(new IBroadcastListener() {
                    @Override
                    public void onHapticPulse(HapticPulse pulse) {
                        for (IBroadcastListener listener : eventListeners) {
                            listener.onHapticPulse(pulse);
                        }
                    }
                });

                broadcasts.startPolling();
            }

            return true;
        }
        catch (Exception x)
        {
            APILogger.error("Error during API connection: " + x);

            // Cleanup possibly connected endpoints
            if(controller != null) controller.dispose();
            if(head != null) head.dispose();
            controller = null;
            head = null;
        }

        return false;
    }

    private void disconnectAllEndpoints(){
        if(head != null) head.dispose();
        if(controller != null) controller.dispose();
        if(broadcasts != null) broadcasts.disconnect();
        head = null;
        controller = null;
    }
}
