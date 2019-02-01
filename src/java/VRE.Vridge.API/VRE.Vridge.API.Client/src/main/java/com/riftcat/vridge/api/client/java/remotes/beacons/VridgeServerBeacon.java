package com.riftcat.vridge.api.client.java.remotes.beacons;

import com.riftcat.vridge.api.client.java.proto.Beacon;

import java.net.InetAddress;

public class VridgeServerBeacon {
    private static long timeoutMs = 5000;

    private Beacon beacon;
    private InetAddress endpoint;
    private long timestmapMs;

    public VridgeServerBeacon(Beacon beacon, InetAddress endpoint) {
        this.beacon = beacon;
        this.endpoint = endpoint;
        timestmapMs = System.currentTimeMillis();
    }

    public Beacon getBeacon() {
        return beacon;
    }

    public InetAddress getEndpoint() {
        return endpoint;
    }

    public boolean isFresh(){
        return timestmapMs + timeoutMs > System.currentTimeMillis();
    }

    @Override
    public String toString() {
        return beacon.getRole() + "|" +
                beacon.getHumanReadableVersion() + "|" +
                beacon.getMachineName() + "|" +
                beacon.getUserName() + "@" +
                endpoint.getHostAddress();
    }
}
