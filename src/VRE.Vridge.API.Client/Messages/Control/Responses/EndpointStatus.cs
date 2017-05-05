namespace VRE.Vridge.API.Client.Messages.Control.Responses
{    
    public class EndpointStatus : ControlResponseHeader
    {
        public string Name;

        public EndpointStatus(string name)
        {
            this.Code = (int) ControlResponseCode.OK;
            this.Name = name;
        }
    }
}
