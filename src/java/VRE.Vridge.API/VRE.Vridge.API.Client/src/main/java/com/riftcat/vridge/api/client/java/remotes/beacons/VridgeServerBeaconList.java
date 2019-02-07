package com.riftcat.vridge.api.client.java.remotes.beacons;

import com.riftcat.vridge.api.client.java.proto.Beacon;

import java.net.InetAddress;
import java.util.ArrayList;
import java.util.Dictionary;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;

public class VridgeServerBeaconList {
    private HashMap<InetAddress, VridgeServerBeacon> timedList = new HashMap<InetAddress, VridgeServerBeacon>();

    public synchronized void add(Beacon beacon, InetAddress endpoint){
        if(timedList.containsKey(endpoint)){
            timedList.remove(endpoint);
        }

        timedList.put(endpoint, new VridgeServerBeacon(beacon, endpoint));
    }

    public synchronized List<VridgeServerBeacon> getFreshServers(){
        LinkedList<VridgeServerBeacon> beacons = new LinkedList<VridgeServerBeacon>();
        for (VridgeServerBeacon vridgeServerBeacon : timedList.values()) {
            if(vridgeServerBeacon.isFresh()){
                beacons.add(vridgeServerBeacon);
            }
        }

        return beacons;
    }
}
