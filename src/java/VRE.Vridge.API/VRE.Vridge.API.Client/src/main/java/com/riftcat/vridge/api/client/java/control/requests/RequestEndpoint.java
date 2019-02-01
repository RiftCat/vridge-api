package com.riftcat.vridge.api.client.java.control.requests;

import com.riftcat.vridge.api.client.java.control.ControlRequestCode;

public class RequestEndpoint extends ControlRequestHeader{

    public String RequestedEndpointName;

    public RequestEndpoint(String name, String appName){
        super(appName);
        Code = ControlRequestCode.RequestEndpoint;
        RequestedEndpointName = name;
    }

}
