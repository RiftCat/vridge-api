namespace VRE.Vridge.API.Client.Messages
{
    // Send this with one zero byte to API service (excluding control) to keep the connection from timing out
    public struct KeepAlive
    {        
        public byte[] Zero;
    }
}
