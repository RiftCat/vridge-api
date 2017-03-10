namespace VRE.Vridge.API.Client.Messages
{
    public enum ControlResponseCode
    {        
        /// <summary>
        /// API awaits connection at given endpoint.
        /// </summary>
        OK = 0,

        /// <summary>
        /// API is not available because of undefined reason.
        /// </summary>
        NotAvailable = 1,

        /// <summary>
        /// API is in use by another client
        /// </summary>
        InUse = 2,

        /// <summary>
        /// Client is trying to use something that requires API client to be updated to more recent version
        /// </summary>
        ClientOutdated = 3,

        /// <summary>
        /// VRidge needs to be updated or client is not following protocol
        /// </summary>
        ServerOutdated = 4,
    }
}
