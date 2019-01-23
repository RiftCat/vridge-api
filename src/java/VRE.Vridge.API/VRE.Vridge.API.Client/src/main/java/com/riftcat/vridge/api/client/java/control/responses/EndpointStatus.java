package com.riftcat.vridge.api.client.java.control.responses;

import com.riftcat.vridge.api.client.java.control.ControlResponseCode;

public class EndpointStatus extends ControlResponseHeader {

    public String Name;
    public String InUseBy;

    public EndpointStatus(String endpointName){
        this.Code = ControlResponseCode.OK;
        this.Name = endpointName;
    }

    public String codeString(){
        if(Code == ControlResponseCode.NotAvailable){
            return "n/a";
        }
        if(Code == ControlResponseCode.InUse){
            return "In use by " + InUseBy;
        }
        if(Code == ControlResponseCode.OK){
            return "Ready";
        }

        return "unknown";
    }

}
