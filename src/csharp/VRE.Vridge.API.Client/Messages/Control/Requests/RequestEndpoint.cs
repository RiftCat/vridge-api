namespace VRE.Vridge.API.Client.Messages.Control.Requests
{
    public class RequestEndpoint : ControlRequestHeader
    {        
        public string RequestedEndpointName;        

        public RequestEndpoint(string name, string requestingAppName)
            : base(requestingAppName, ControlRequestCode.RequestEndpoint)
        {            
            RequestedEndpointName = name;            
        }
    }
}
