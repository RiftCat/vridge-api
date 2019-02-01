package com.riftcat.vridge.api.client.java;

public enum Capabilities {
    HeadTracking(1),
    Controllers(2);

    private final int value;

    Capabilities(int value) {
        this.value = value;
    }
}
