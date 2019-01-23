package com.riftcat.vridge.api.client.java.utils;

import com.google.gson.Gson;

import org.zeromq.ZMQ;

public class SocketHelpers {

    public static Gson Serializer;

    static{
        Serializer = new Gson();
    }

    public static boolean SendAsJson(ZMQ.Socket socket, Object obj){

        String json = Serializer.toJson(obj);
        return socket.send(json);
    }

    public static <T> T ReceiveByJson(ZMQ.Socket socket, Class<T> type){
        String json = socket.recvStr();
        Object obj = Serializer.fromJson(json, type);

        return (T) obj;

    }
}
