package com.riftcat.vridge.api.client.java.remotes;

import com.riftcat.vridge.api.client.java.proxy.ClientProxyBase;
import com.riftcat.vridge.api.client.java.proxy.VRidgeApiProxy;

class RemoteBase {

    private boolean isDisposed;
    private VRidgeApiProxy proxy;

    protected RemoteBase(VRidgeApiProxy proxy){
        this.proxy = proxy;
    }

    public boolean isDisposed() {
        return isDisposed;
    }

    public void dispose(){
        isDisposed = true;
        if(proxy != null){
            proxy.disconnect();
        }
    }
}
