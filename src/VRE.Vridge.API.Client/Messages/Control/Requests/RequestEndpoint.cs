namespace VRE.Vridge.API.Client.Messages.Control.Requests
{
    public class RequestEndpoint : ControlRequestHeader
    {        
        public string RequestedEndpointName;

        public string RequestingAppName;

        public RequestEndpoint(string name, string requestingAppName)
        {
            Code = (int) ControlRequestCode.RequestEndpoint;
            RequestedEndpointName = name;
            RequestingAppName = requestingAppName;
        }
    }
}
