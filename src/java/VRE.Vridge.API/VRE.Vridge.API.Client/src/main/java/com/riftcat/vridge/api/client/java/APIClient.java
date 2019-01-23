package com.riftcat.vridge.api.client.java;

import com.riftcat.vridge.api.client.java.control.ControlRequestCode;
import com.riftcat.vridge.api.client.java.control.ControlResponseCode;
import com.riftcat.vridge.api.client.java.control.requests.ControlRequestHeader;
import com.riftcat.vridge.api.client.java.control.requests.RequestEndpoint;
import com.riftcat.vridge.api.client.java.control.responses.APIStatus;
import com.riftcat.vridge.api.client.java.control.responses.EndpointCreated;
import com.riftcat.vridge.api.client.java.proxy.BroadcastProxy;
import com.riftcat.vridge.api.client.java.proxy.ClientProxyBase;
import com.riftcat.vridge.api.client.java.proxy.ControllerProxy;
import com.riftcat.vridge.api.client.java.proxy.HeadTrackingProxy;
import com.riftcat.vridge.api.client.java.proxy.VRidgeApiProxy;
import com.riftcat.vridge.api.client.java.utils.ILog;
import com.riftcat.vridge.api.client.java.utils.SocketHelpers;

import org.zeromq.ZContext;
import org.zeromq.ZMQ;

import java.util.HashMap;
import java.util.LinkedList;
import java.util.concurrent.TimeoutException;

public class APIClient {

    public final static int HEADTRACKING = 0;
    public final static int CONTROLLER = 1;
    public final static int BROADCASTS = 2;

    public static ZContext ZContext;

    private HashMap<Integer, VRidgeApiProxy> proxies;
    private String serverAddress = "tcp://localhost";

    // Connections with same app name will not result in "endpoint in use" response
    private String appName = "";

    public APIClient(String appName){
        ZContext = new ZContext(4);
        proxies = new HashMap<Integer, VRidgeApiProxy>();

        this.appName = appName;
    }

    public APIClient(String ip, String appName){
        this(appName);
        serverAddress = ip;
    }

    /// <summary>
    /// Sends control request to see what APIs are available.
    /// May return null if control connection dies (automatic reconnect will follow).
    /// </summary>
    public APIStatus GetStatus() throws Exception {

        ZMQ.Socket controlSocket = createControlSocket();
        if (controlSocket == null)
        {
            return null;
        }

        SocketHelpers.SendAsJson(controlSocket, new ControlRequestHeader(ControlRequestCode.RequestStatus));
        APIStatus status = SocketHelpers.ReceiveByJson(controlSocket, APIStatus.class);
        APIClient.ZContext.destroySocket(controlSocket);

        if (status != null){
            return status;
        }

        throw new Exception("Could not read API status.");
    }

    public <T extends VRidgeApiProxy> T getProxy(int proxyType) throws Exception {

        VRidgeApiProxy proxy = proxies.get(proxyType);
        ZMQ.Socket controlSocket = createControlSocket();

        if(proxy == null){

            String endpointName = null;
            switch (proxyType)
            {
                case HEADTRACKING:
                    endpointName = EndpointNames.HeadTracking;
                    break;
                case CONTROLLER:
                    endpointName = EndpointNames.Controller;
                    break;
                case BROADCASTS:
                    endpointName = EndpointNames.Broadcast;
                    break;
            }

            if(endpointName == null){{
                throw new IllegalArgumentException("Invalid proxy type was requested.");
            }}

            SocketHelpers.SendAsJson(controlSocket, new RequestEndpoint(endpointName, appName));
            EndpointCreated response = SocketHelpers.ReceiveByJson(controlSocket, EndpointCreated.class);
            APIClient.ZContext.destroySocket(controlSocket);

            if(response == null ){
                throw new TimeoutException("API server timeout");
            }

            if(response.Code == ControlResponseCode.InUse){
                throw new Exception("API endpoint in use.");
            }

            switch (proxyType){
                case HEADTRACKING:
                    proxies.put(proxyType, new HeadTrackingProxy("tcp://" + serverAddress + ":" + response.Port, true));
                    break;
                case CONTROLLER:
                    proxies.put(proxyType, new ControllerProxy("tcp://" + serverAddress + ":" + response.Port));
                    break;
                case BROADCASTS:
                    proxies.put(proxyType, new BroadcastProxy("tcp://" + serverAddress + ":" + response.Port));
                    break;
            }
        }

        return (T) proxies.get(proxyType);
    }

    public void disconnectProxy(int proxyType)
    {
        VRidgeApiProxy proxy = proxies.get(proxyType);

        if (proxy == null) return;

        proxy.disconnect();
        proxies.put(proxyType, null);
    }

    public void disconnectAll() {
        for(int proxyId : proxies.keySet()){
            disconnectProxy(proxyId);
        }
    }

    private String getEndpointAddress() {

        return "tcp://" + serverAddress + ":38219";
    }

    private ZMQ.Socket createControlSocket(){
        String ctrlAddress = getEndpointAddress();

        ZMQ.Socket controlSocket = ZContext.createSocket(ZMQ.REQ);
        controlSocket.connect(ctrlAddress);
        controlSocket.setSendTimeOut(1000);
        controlSocket.setReceiveTimeOut(1000);
        return controlSocket;
    }
}
