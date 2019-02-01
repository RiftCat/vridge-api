using Newtonsoft.Json;

namespace VRE.Vridge.API.Client.Messages.Control.Responses
{    
    public class EndpointStatus : ControlResponseHeader
    {
        [JsonProperty("Name")]
        public string Name;

        [JsonProperty("InUseBy")]
        public string InUseBy;

        public EndpointStatus(string endpointName) : base(ControlResponseCode.OK)
        {
            this.Name = endpointName;
        }

        public override string ToString()
        {
            if (Code == (int) ControlResponseCode.InUse)
            {
                return Name + ": " + (ControlResponseCode)Code + " by " + InUseBy;
            }
            else
            {
                return Name + ": " + (ControlResponseCode)Code;
            }
            
        }
    }
}
