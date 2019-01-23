package com.riftcat.vridge.api.client.java.control.responses;

import com.riftcat.vridge.api.client.java.EndpointNames;

import java.util.List;

public class APIStatus {

    public List<EndpointStatus> Endpoints;

    @Override
    public String toString() {

        String strStatus = "";

        for(EndpointStatus status : Endpoints){
            strStatus += status.Name + "(" + status.codeString() + ") | ";
        }

        return strStatus;
    }

    public EndpointStatus findEndpoint(String name){
        for (EndpointStatus endpoint : Endpoints) {
            if(endpoint.Name.equals(name)){
                return endpoint;
            }
        }

        return null; }

}
