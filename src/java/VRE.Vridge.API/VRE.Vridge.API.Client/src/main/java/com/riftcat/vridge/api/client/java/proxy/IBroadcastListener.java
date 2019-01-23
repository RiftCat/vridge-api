package com.riftcat.vridge.api.client.java.proxy;

import com.riftcat.vridge.api.client.java.proto.HapticPulse;

public interface IBroadcastListener{
    void onHapticPulse(HapticPulse pulse);
}
