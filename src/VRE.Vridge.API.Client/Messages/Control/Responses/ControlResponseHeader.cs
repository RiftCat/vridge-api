namespace VRE.Vridge.API.Client.Messages.Control.Responses
{
    public class ControlResponseHeader : BaseControlMessage
    {   
        // Predefined responses
        public static ControlResponseHeader ResponseInUse = new ControlResponseHeader()
        {
            Code = (int) ControlResponseCode.InUse
        };

        public static ControlResponseHeader ResponseClientOutdated = new ControlResponseHeader()
        {
            Code = (int) ControlResponseCode.ClientOutdated
        };

        public static ControlResponseHeader ResponseNotAvailable = new ControlResponseHeader()
        {
            Code = (int) ControlResponseCode.NotAvailable
        };
    }
}
