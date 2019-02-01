namespace VRE.Vridge.API.Client.Messages.Control.Responses
{
    public class ControlResponseHeader : BaseControlMessage
    {   
        // Predefined responses
        public static ControlResponseHeader ResponseInUse = 
            new ControlResponseHeader(ControlResponseCode.InUse);

        public static ControlResponseHeader ResponseClientOutdated =
            new ControlResponseHeader(ControlResponseCode.ClientOutdated);

        public static ControlResponseHeader ResponseNotAvailable =
            new ControlResponseHeader(ControlResponseCode.NotAvailable);

        public static ControlResponseHeader ResponseServerOutdated =
            new ControlResponseHeader(ControlResponseCode.ServerOutdated);

        public ControlResponseHeader(ControlResponseCode code) : base(3, (int) code)
        {
        }
    }
}
