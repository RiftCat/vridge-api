namespace VRE.Vridge.API.Client.Messages.Control.Requests
{
    public class ControlRequestHeader : BaseControlMessage
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
