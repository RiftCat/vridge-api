namespace VRE.Vridge.API.Client.Messages.v3
{
    public enum TrackedDeviceStatus
    {        
        /// <summary>
        /// Currently tracked.
        /// </summary>
        Active = 0,

        /// <summary>
        /// Not in tracking range or tracking unavailable due to other reasons.
        /// </summary>
        TempUnavailable = 1,

        /// <summary>
        /// Permamently gone.
        /// </summary>
        Disabled = 2
    }
}