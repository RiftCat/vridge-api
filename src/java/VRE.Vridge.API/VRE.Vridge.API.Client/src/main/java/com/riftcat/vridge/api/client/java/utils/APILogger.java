package com.riftcat.vridge.api.client.java.utils;

import java.util.LinkedList;

public class APILogger {

    private static LinkedList<ILog> loggers = new LinkedList<ILog>();

    public static void AddLogListener(ILog logger){
        loggers.add(logger);
    }

    public static void debug(String s){
        for (ILog log : loggers) {
            log.debug(s);
        }
    }

    public static void zmq(String s) {
        //debug("ZMQ: " + s);
    }

    public static void error(String s) {
        for (ILog log : loggers) {
            log.error(s);
        }
    }
}
