package com.riftcat.vridge.api.client.java.control;

public class ControlResponseCode {

    /// <summary>
    /// API awaits connection at given endpoint.
    /// </summary>
    public static int OK = 0;

    /// <summary>
    /// API is not available because of undefined reason.
    /// </summary>
    public static int NotAvailable = 1;

    /// <summary>
    /// API is in use by another client
    /// </summary>
    public static int InUse = 2;

    /// <summary>
    /// Client is trying to use something that requires API client to be updated to more recent version
    /// </summary>
    public static int ClientOutdated = 3;

    /// <summary>
    /// VRidge needs to be updated or client is not following protocol
    /// </summary>
    public static int ServerOutdated = 4;

}
