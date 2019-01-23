package com.riftcat.vridge.api.client.java.utils;

import com.google.protobuf.ByteString;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class SerializationUtils {

    public static ByteString byteStringFromFloats(float...args){
        return ByteString.copyFrom(byteArrayFromFloats(args));
    }

    public static byte[] byteArrayFromFloats(float... args){
        ByteBuffer data = ByteBuffer.allocate(args.length * 4);
        data.order(ByteOrder.LITTLE_ENDIAN);
        for (float arg : args) {
            data.putFloat(arg);
        }

        return data.array();
    }

    public static ByteString byteStringFromFloatArray(float[] array){
        ByteBuffer data = ByteBuffer.allocate(array.length * 4);
        data.order(ByteOrder.LITTLE_ENDIAN);
        for (float arg : array) {
            data.putFloat(arg);
        }

        return ByteString.copyFrom(data.array());
    }
}
