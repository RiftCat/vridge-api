using System;
using NetMQ.Sockets;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages;
using VRE.Vridge.API.Client.Messages.v1.Control.Requests;
using VRE.Vridge.API.Client.Messages.v1.Control.Responses;
using VRE.Vridge.API.Client.Proxy.Controller;
using VRE.Vridge.API.Client.Proxy.HeadTracking;

namespace VRE.Vridge.API.Client.Proxy
{
    public class APIClient
    {
        private RequestSocket     controlSocket;
        private HeadTrackingProxy headTrackingProxy;
        private ControllerProxy   controllerProxy;

        private readonly string endpointAddress;

        public APIClient()
        {            
            endpointAddress = "tcp://localhost:38219";
            Connect();            
        }

        public APIClient(string ip)
        {            
            endpointAddress = $"tcp://{ip}:38219";
            Connect();
        }

        /// <summary>
        /// Sends control request to see what APIs are available. 
        /// May return null if control conenction dies (automatic reconnect will follow). 
        /// </summary>        
        public APIStatus GetStatus()
        {
            APIStatus status;

            if (controlSocket == null)
            {
                return null;
            }

            controlSocket.SendAsJson(new ControlRequestHeader(ControlRequestCode.RequestStatus));            
            var success = controlSocket.TryReceiveJson(out status, 500);

            if (success)
            {
                return status;
            }

            // Reconnect if something goes wrong
            controlSocket.Close();
            Connect();

            return null;
        }

        public HeadTrackingProxy ConnectHeadTrackingProxy()
        {
            if (headTrackingProxy == null)
            {
                // Request headtracking endpoint address
                bool success = controlSocket.TrySendAsJson(new RequestEndpoint(EndpointNames.HeadTracking), 1000);

                if (!success)
                    HandleControlConnectionException(new Exception("API server timeout."));

                EndpointCreated response;
                success = controlSocket.TryReceiveJson(out response, 1000);

                if (!success)
                    HandleControlConnectionException(new Exception("API server timeout."));

                if (response.Code == (int) ControlResponseCode.InUse)
                    HandleControlConnectionException(new Exception("API already in use by another client."));

                // Initialize the proxy
                headTrackingProxy = new HeadTrackingProxy(response.EndpointAddress, false);
            }

            return headTrackingProxy;
        }

        public ControllerProxy ConnectToControllerProxy()
        {
            if (controllerProxy == null)
            {
                // Request headtracking endpoint address
                bool success = controlSocket.TrySendAsJson(new RequestEndpoint(EndpointNames.Controller), 1000);

                if (!success)
                    HandleControlConnectionException(new Exception("API server timeout."));

                EndpointCreated response;
                success = controlSocket.TryReceiveJson(out response, 1000);

                if (!success)
                    HandleControlConnectionException(new Exception("API server timeout."));

                if (response.Code == (int)ControlResponseCode.InUse)
                    HandleControlConnectionException(new Exception("API already in use by another client."));

                // Initialize the proxy
                controllerProxy = new ControllerProxy(response.EndpointAddress, false);
            }

            return controllerProxy;
        }

        // TODO Refactor 2 methods above (DRY)

        /// <summary>
        /// Closes head tracking API connection and lets other API clients use it.
        /// </summary>
        public void DisconnectHeadTrackingProxy()
        {
            if (headTrackingProxy == null) return;

            headTrackingProxy.Disconnect();
            headTrackingProxy = null;                                   
        }

        /// <summary>
        /// Closes controller API connection and lets other API clients use it.
        /// </summary>
        public void DisconnectControllerProxy()
        {
            if (controllerProxy == null) return;

            controllerProxy.Disconnect();
            controllerProxy = null;
        }
        
        private void Connect()
        {
            controlSocket = new RequestSocket();            
            controlSocket.Connect(endpointAddress);
        }

        private void HandleControlConnectionException(Exception x)
        {
            // Reset socket state to continue accepting new requests*            
            controlSocket.Close();
            controlSocket = new RequestSocket();
            controlSocket.Connect(endpointAddress);

            // Let client know that something went wrong
            throw x;

            /* *This can be also done with ZMQ_REQ_RELAXED + ZMQ_REQ_CORRELATE
               options instead of brute-force-like restarting the socket    
               but NetMQ does not seem to support these options */
        }
    }
}
