package com.riftcat.vridge.api.client.java.proxy;

import com.google.protobuf.InvalidProtocolBufferException;
import com.riftcat.vridge.api.client.java.APIClient;
import com.riftcat.vridge.api.client.java.proto.HapticPulse;
import com.riftcat.vridge.api.client.java.utils.APILogger;

import org.zeromq.ZMQ;
import java.nio.charset.Charset;
import java.util.LinkedList;
import java.util.List;


public class BroadcastProxy implements VRidgeApiProxy {

    private final String endpointAddr;
    private ZMQ.Socket socket;
    private List<IBroadcastListener> listeners = new LinkedList<IBroadcastListener>();

    private Thread threadPolling;

    public BroadcastProxy(String endpointAddr){
        this.endpointAddr = endpointAddr;
    }

    public void startPolling(){

        if(threadPolling != null) threadPolling.interrupt();

        threadPolling = new Thread(new Runnable() {
            @Override
            public void run() {

                socket = APIClient.ZContext.createSocket(ZMQ.SUB);
                socket.connect(endpointAddr);
                socket.subscribe("haptic".getBytes(Charset.forName("UTF-8")));
                socket.setLinger(1000);

                ZMQ.Poller poller = APIClient.ZContext.createPoller(1);
                poller.register(socket, ZMQ.Poller.POLLIN);

                while(!Thread.currentThread().isInterrupted()){

                    int result = poller.poll(1000);
                    if(result > 0){

                        // we can ignore topic here, it's filtered at socket level
                        // but we need to consume it from socket to continue
                        socket.recvStr();

                        byte[] bufMsg = socket.recv();

                        try {

                            // Deserialize
                            HapticPulse pulse = HapticPulse.parseFrom(bufMsg);

                            // Notify listeners
                            for(IBroadcastListener listener : listeners){
                                listener.onHapticPulse(pulse);
                            }

                        } catch (InvalidProtocolBufferException e) {
                            // Invalid data - could not be deserialized
                        }
                    }
                } //  while(!Thread.currentThread().isInterrupted())

                poller.close();
            }
        });

        threadPolling.start();
    }

    public void disconnect(){
        try {
            if(threadPolling != null){
                threadPolling.interrupt();
                threadPolling.join();
            }

            if(socket != null){
                socket.close();
            }

            listeners.clear();
        } catch (InterruptedException e) {
            APILogger.error("Can't close Broadcast endpoint.");
            e.printStackTrace();
        }

    }

    public void addListener(IBroadcastListener listener){
        listeners.add(listener);
    }

    public void removeListener(IBroadcastListener listener){
        listeners.remove(listener);
    }

}

