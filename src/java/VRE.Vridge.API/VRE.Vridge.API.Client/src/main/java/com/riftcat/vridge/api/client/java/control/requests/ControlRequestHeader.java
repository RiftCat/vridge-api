package com.riftcat.vridge.api.client.java.control.requests;

import com.riftcat.vridge.api.client.java.control.BaseControlMessage;
import com.riftcat.vridge.api.client.java.control.ControlRequestCode;

public class ControlRequestHeader extends BaseControlMessage {
    public String RequestingAppName;

    public ControlRequestHeader(String requestingAppName){
        RequestingAppName = requestingAppName;
    }

    public ControlRequestHeader(int reqCode){
        Code = reqCode;
    }
}
