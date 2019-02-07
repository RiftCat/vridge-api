package com.riftcat.vridge.api.client.java.remotes;

import com.google.protobuf.InvalidProtocolBufferException;
import com.riftcat.vridge.api.client.java.proto.Beacon;
import com.riftcat.vridge.api.client.java.proto.BeaconOrigin;
import com.riftcat.vridge.api.client.java.remotes.beacons.VridgeServerBeacon;
import com.riftcat.vridge.api.client.java.remotes.beacons.VridgeServerBeaconList;

import org.zeromq.ZBeacon;

import java.net.InetAddress;
import java.util.List;

class DiscoveryClient implements Thread.UncaughtExceptionHandler {

    private final byte[] identity;
    private VridgeServerBeaconList beaconList;
    private ZBeacon beaconClient;

    DiscoveryClient(){

        identity = Beacon.newBuilder()
                .setRole(BeaconOrigin.Client)
                // We don't use information below
                .setMachineName("Android")
                .setHumanReadableVersion("Android")
                .setUserName("Android")
                .build()
                .toByteArray();

        beaconList = new VridgeServerBeaconList();
        reset();
    }

    public void reset(){
        dispose();
        beaconClient = new ZBeacon("255.255.255.255",38219, identity, true, true);
        beaconClient.setBroadcastInterval(1000);
        beaconClient.setListener(new ZBeacon.Listener() {
            @Override
            public void onBeacon(InetAddress sender, byte[] buffer) {

                Beacon beacon;
                try {
                    beacon = Beacon.parser().parseFrom(buffer);
                } catch (InvalidProtocolBufferException e) {
                    // Ignore stray packets
                    return;
                }

                if(beacon.getRole() != BeaconOrigin.Server){
                    // Skip other clients
                    return;
                }

                beaconList.add(beacon, sender);
            }
        });
        beaconClient.setUncaughtExceptionHandlers(this, this);
        beaconClient.start();
    }

    public List<VridgeServerBeacon> getFreshServers() {
        return beaconList.getFreshServers();
    }

    public synchronized void dispose() {
        try {
            if(beaconClient != null){
                beaconClient.stop();
                beaconClient = null;
            }
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    @Override
    public void uncaughtException(Thread t, Throwable e) {
        try {
            Thread.sleep(3000);
        } catch (InterruptedException ignored) {

        }

        reset();
    }
}
