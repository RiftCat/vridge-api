namespace VRE.Vridge.API.Client.Messages.v1.Control.Requests
{
    public class ControlRequestHeader : BaseMessage
    {                
        public ControlRequestHeader()
        {
            
        }

        public ControlRequestHeader(ControlRequestCode code)
        {
            Code = (int) code;            
        }
    }
}
