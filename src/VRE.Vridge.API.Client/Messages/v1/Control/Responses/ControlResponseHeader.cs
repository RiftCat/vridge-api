namespace VRE.Vridge.API.Client.Messages.v1.Control.Responses
{
    public class ControlResponseHeader : BaseMessage
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
