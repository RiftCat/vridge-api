namespace VRE.Vridge.API.Client.Messages.Control.Requests
{
    public class ControlRequestHeader : BaseControlMessage
    {
        public string RequestingAppName;
        
        public ControlRequestHeader(string requestingAppName, ControlRequestCode code) : base(3, (int)code)
        {
            RequestingAppName = requestingAppName;            
        }

        public ControlRequestHeader() : base()
        {
            // Deserialization only
        }
    }
}
