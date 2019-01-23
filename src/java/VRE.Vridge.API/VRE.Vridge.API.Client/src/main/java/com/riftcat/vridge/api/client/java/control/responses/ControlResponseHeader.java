package com.riftcat.vridge.api.client.java.control.responses;

import com.riftcat.vridge.api.client.java.control.BaseControlMessage;
import com.riftcat.vridge.api.client.java.control.ControlResponseCode;

public class ControlResponseHeader extends BaseControlMessage {

    // Predefined responses
    public static ControlResponseHeader ResponseInUse;
    public static ControlResponseHeader ResponseClientOutdated;
    public static ControlResponseHeader ResponseNotAvailable;

    static{
        // Predefined responses
        ResponseNotAvailable  = new ControlResponseHeader();
        ResponseNotAvailable.Code = ControlResponseCode.NotAvailable;

        ResponseClientOutdated  = new ControlResponseHeader();
        ResponseClientOutdated.Code = ControlResponseCode.ClientOutdated;

        ResponseInUse  = new ControlResponseHeader();
        ResponseInUse.Code = ControlResponseCode.InUse;
    }

}
