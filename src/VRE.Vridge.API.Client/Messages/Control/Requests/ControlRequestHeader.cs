namespace VRE.Vridge.API.Client.Messages.Control.Requests
{
    public class ControlRequestHeader : BaseControlMessage
    {                
        public ControlRequestHeader() : base(3)
        {
            
        }

        public ControlRequestHeader(ControlRequestCode code) : base(3)
        {
            Code = (int) code;            
        }
    }
}
