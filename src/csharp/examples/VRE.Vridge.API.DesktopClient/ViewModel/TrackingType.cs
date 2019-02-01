namespace VRE.Vridge.API.DesktopTester.ViewModel
{
    /// <summary>
    /// Tracking mode which decides how to use user values.
    /// </summary>
    public enum TrackingType
    {       
        Position,
        PositionAndRotation,        
        SyncOffset,
        AsyncOffset
    }
}