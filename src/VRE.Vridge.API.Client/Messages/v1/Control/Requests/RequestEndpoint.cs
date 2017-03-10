namespace VRE.Vridge.API.Client.Messages.v1.Control.Requests
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
