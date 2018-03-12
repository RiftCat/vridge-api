using System;
using System.Collections.Generic;
using NetMQ.Sockets;
using VRE.Vridge.API.Client.Helpers;
using VRE.Vridge.API.Client.Messages;
using VRE.Vridge.API.Client.Messages.Control;
using VRE.Vridge.API.Client.Messages.Control.Requests;
using VRE.Vridge.API.Client.Messages.Control.Responses;
using VRE.Vridge.API.Client.Proxy.Broadcasts;
using VRE.Vridge.API.Client.Proxy.Controller;
using VRE.Vridge.API.Client.Proxy.HeadTracking;

namespace VRE.Vridge.API.Client.Proxy
{
    public class APIClient
    {
        public enum Endpoints
        {
            HeadTracking,
            Controller,
            Broadcast
        }

        private RequestSocket     controlSocket;        
        
        private readonly string serverAddress = "localhost";
        private readonly string appName = "default";

        public APIClient(string appName)
        {
            this.appName = appName;            
        }

        public APIClient(string ip, string appName)
        {
            serverAddress = ip;
            this.appName = appName;            
        }

        /// <summary>
        /// Sends control request to see what APIs are available. 
        /// May return null if control connection dies (automatic reconnect will follow). 
        /// </summary>        
        public APIStatus GetStatus()
        {
            APIStatus status;

            ConnectToControlSocket();

            controlSocket.SendAsJson(new ControlRequestHeader(ControlRequestCode.RequestStatus));            
            var success = controlSocket.TryReceiveJson(out status, 500);

            controlSocket.Close();

            if (success)
            {
                return status;
            }            

            return null;
        }

        public T CreateProxy<T>(bool keepAlive = true)
        {            
            // Find out which endpoint name should be used
            string endpointName;
            if (typeof(T) == typeof(HeadTrackingProxy))
            {
                endpointName = EndpointNames.HeadTracking;
            }
            else if (typeof(T) == typeof(ControllerProxy))
            {
                endpointName = EndpointNames.Controller;
            }
            else if (typeof(T) == typeof(BroadcastProxy))
            {
                endpointName = EndpointNames.Broadcast;
            }
            else
            {
                throw new ArgumentException("Invalid proxy requested.");
            }

            ConnectToControlSocket();

            // Try requesting given endpoint
            bool success = controlSocket.TrySendAsJson(new RequestEndpoint(endpointName, appName), 1000);

            if (!success)
            {
                HandleControlConnectionException(new Exception("API server timeout."));
            }
                

            EndpointCreated response;
            success = controlSocket.TryReceiveJson(out response, 1000);

            controlSocket.Close();

            if (!success)
            {
                HandleControlConnectionException(new Exception("API server timeout."));
            }

            if (response.Code == (int) ControlResponseCode.InUse)
            {
                HandleControlConnectionException(new Exception("API already in use by another client."));
            }

            // Initialize the proxy of requested type
            var connectionString = $"tcp://{serverAddress}:{response.Port}";
            if (typeof(T) == typeof(HeadTrackingProxy))
            {
                return (T) Convert.ChangeType(new HeadTrackingProxy(connectionString, keepAlive), typeof(T));
            }
            if (typeof(T) == typeof(ControllerProxy))
            {
                return (T) Convert.ChangeType(new ControllerProxy(connectionString, keepAlive), typeof(T));
            }
            if (typeof(T) == typeof(BroadcastProxy))
            {
                return (T) Convert.ChangeType(new BroadcastProxy(connectionString), typeof(T));
            }

            throw new ArgumentException("Invalid proxy requested.");
        }

        private void ConnectToControlSocket()
        {
            controlSocket = new RequestSocket();            
            controlSocket.Connect(ControlEndpoint);
        }

        protected void HandleControlConnectionException(Exception x)
        {
            // Reset socket state to continue accepting new requests*            
            controlSocket.Close();
            controlSocket = new RequestSocket();
            controlSocket.Connect(ControlEndpoint);

            // Let client know that something went wrong
            throw x;

            /* *This can be also done with ZMQ_REQ_RELAXED + ZMQ_REQ_CORRELATE
               options instead of brute-force-like restarting the socket    
               but NetMQ does not seem to support these options */
        }

        private string ControlEndpoint => $"tcp://{serverAddress}:38219";
    }
}
