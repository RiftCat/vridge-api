package com.riftcat.vridge.api.client.java.proxy;

import com.google.protobuf.InvalidProtocolBufferException;
import com.google.protobuf.MessageLite;
import com.google.protobuf.Parser;
import com.riftcat.vridge.api.client.java.APIClient;
import com.riftcat.vridge.api.client.java.utils.APILogger;

import org.zeromq.ZContext;
import org.zeromq.ZMQ;

import java.util.TimerTask;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

public abstract class ClientProxyBase implements VRidgeApiProxy{

    private TimerTask keepAliveTimer;
    private Runnable keepAlivePing;

    private byte[] keepAlivePacket = { 0 };

    int CurrentVersion = 3;
    ZMQ.Socket socket;

    ClientProxyBase(String endpointAddress, boolean keepAlive){

        if(APIClient.ZContext == null) APIClient.ZContext = new ZContext(4);
        socket = APIClient.ZContext.createSocket(ZMQ.REQ);
        socket.setLinger(1000);
        socket.setSendTimeOut(15000);
        socket.setReceiveTimeOut(15000);
        socket.connect(endpointAddress);
        socket.setHWM(1);

        if (!keepAlive) return;

        keepAlivePing = new Runnable() {
            @Override
            public void run() {
                sendKeepAlivePing();
            }
        };

        ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
        executor.scheduleAtFixedRate(keepAlivePing, 1, 5, TimeUnit.SECONDS);
    }

    void CloseSocket(){
        APIClient.ZContext.destroySocket(socket);
    }

    synchronized <T extends MessageLite> T SendMessage(MessageLite msg, Parser<T> parser) throws TimeoutException {

        APILogger.zmq("send begin");
        long timestamp = System.nanoTime();
        socket.send(msg.toByteArray());
        byte[] responseBytes = socket.recv();
        APILogger.zmq("recv end - " + (System.nanoTime() - timestamp) / 1000000.0);

        if (responseBytes != null){
            try {
                T response = parser.parseFrom(responseBytes);
                return response;
            } catch (InvalidProtocolBufferException e) {
                // whoops
            }
        }

        APILogger.zmq("timeout");
        APIClient.ZContext.destroySocket(socket);
        throw new TimeoutException();
    }

    public abstract void disconnect();

    public synchronized boolean sendKeepAlivePing(){
        boolean error = false;
        APILogger.zmq("ping begin: ");
        error = error || !socket.send(keepAlivePacket);
        error = error || socket.recv() == null;
        APILogger.zmq("ping end - error: " + error);

        return !error;
    }
}
