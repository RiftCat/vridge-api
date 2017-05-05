namespace VRE.Vridge.API.Client.Messages.Control.Requests
{
    public class RequestEndpoint : ControlRequestHeader
    {        
        public string RequestedEndpointName;

        public RequestEndpoint(string name)
        {
            Code = (int) ControlRequestCode.RequestEndpoint;
            RequestedEndpointName = name;
        }
    }
}
