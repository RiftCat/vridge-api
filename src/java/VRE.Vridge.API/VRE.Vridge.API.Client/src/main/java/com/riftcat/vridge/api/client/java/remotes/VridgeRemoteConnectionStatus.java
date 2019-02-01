package com.riftcat.vridge.api.client.java.remotes;

public enum VridgeRemoteConnectionStatus {

    /** Connected or will be auto-connected on first call */
    Okay,
    Unreachable,
    InUse,
    UnexpectedError
}
