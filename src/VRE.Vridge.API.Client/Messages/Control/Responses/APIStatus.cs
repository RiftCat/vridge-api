using System.Collections.Generic;

namespace VRE.Vridge.API.Client.Messages.Control.Responses
{
    /// <summary>
    /// Contains list of available (or not) API services.
    /// </summary>
    public class APIStatus : ControlResponseHeader
    {        
        public List<EndpointStatus> Endpoints;
    }
}
