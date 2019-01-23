using ProtoBuf;


namespace VRE.Vridge.API.Client.Messages.Control
{
    public class BaseControlMessage
    {
        public int ProtocolVersion;
        public int Code;
        
        public BaseControlMessage(int versionCode, int code)
        {
            ProtocolVersion = versionCode;
            Code = code;
        }

        public BaseControlMessage()
        {
            // Deserialization only
        }
        
    }
}